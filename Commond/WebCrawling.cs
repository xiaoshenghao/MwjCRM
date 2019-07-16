using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using Tesseract.Interop;
using Newtonsoft;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using WJewel.DataContract.WJewel.Common;
using System.Threading;
using WJewel.Basic;
using WJewel.DataContract.Common;
using CRM.Customer.Data;
using CRM.Customer.Domain;

namespace CRM.Customer.Service
{
    public class WebCrawling
    {
        /// <summary>
        /// 网站host
        /// </summary>
        private string host = string.Empty;
        /// <summary>
        /// 爬取根目录
        /// </summary>
        private string urlbase = string.Empty;
        /// <summary>
        /// 验证softKey编号，登录验证用
        /// </summary>
        private string softKeyId = string.Empty;
        /// <summary>
        /// 登录验证用
        /// </summary>
        private string uuid = string.Empty;
        /// <summary>
        /// 登录验证用
        /// </summary>
        private string uuidEncode = string.Empty;
        /// <summary>
        /// Mac地址，登录验证用
        /// </summary>
        private string macStr = string.Empty;
        private string loginMode = "1";
        /// <summary>
        /// 帐号
        /// </summary>
        private string userNo = string.Empty;
        /// <summary>
        /// 密码
        /// </summary>
        private string userPass = string.Empty;
        private string randomCode = "";
        /// <summary>
        /// 登录后统一用该cookie作为凭证
        /// </summary>
        private string cookie = "";
        /// <summary>
        /// 抓取失败重试次数
        /// </summary>
        private int maxCount = 3;
        /// <summary>
        /// 过期时间，便于识别
        /// </summary>
        private int expire = 24 * 60 * 60;

        private HttpHelper httpHelper = new HttpHelper();
        private HttpItem httpItem = new HttpItem();
        private HttpResult httpResult;
        private string uploadFilePath = "\\temp\\";
        /// <summary>
        /// 登录状态，如果初始化登录失败，需要手动调用Login方法登录
        /// </summary>
        public bool LoginStatus { get; } = false;
        /// <summary>
        /// 是否配置/启用同步功能
        /// </summary>
        public bool IsSync { get; } = false;

        public Userinfo userinfo;
        /// <summary>
        /// 构造函数，根据商户编号初始化爬虫
        /// </summary>
        /// <param name="mctNum"></param>
        public WebCrawling(string mctNum)
        {
            var par = RedisService.Instance.HashGet<Sync_ParSet>(RedisPrimaryKey.WebCrawlingMctInfo, mctNum);
            if (par == null)
            {
                par = new SyncParSet().GetInfoByMctNum(mctNum, "erp");

                if (par != null)
                {
                    var timeSpan = (DateTime.Now.AddMilliseconds(expire * 1000) - DateTime.Now);
                    RedisService.Instance.HashSet<Sync_ParSet>(RedisPrimaryKey.WebCrawlingMctInfo, mctNum, par, timeSpan);
                }
                if (par == null || !par.IsEnble)
                {
                    IsSync = false;
                }
                else
                {
                    host = par.Host;
                    urlbase = par.UrlBase;
                    softKeyId = par.SoftKeyId;
                    uuid = par.Uuid;
                    uuidEncode = par.UuidEncode;
                    macStr = par.MacStr;
                    loginMode = par.LoginMode;
                    userNo = par.UserNo;
                    userPass = par.UserPass;

                    IsSync = true;

                    Init();
                }
            }
            else
            {
                IsSync = par.SystemType == "erp" && par.IsEnble;
            }
        }

        /// <summary>
        /// 初始化，并自动登录
        /// </summary>
        public void Init()
        {

            httpItem.URL = $"{urlbase}/login.do";
            httpItem.ResultType = ResultType.String;
            httpItem.Host = host;
            httpItem.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";

            cookie = RedisService.Instance.StringGet($"{RedisPrimaryKey.WebCrawlingCookie}/{uuid}");

            userinfo = RedisService.Instance.HashGet<Userinfo>(RedisPrimaryKey.WebCrawlingUserInfo, uuid);

            if (string.IsNullOrEmpty(cookie))
            {
                Login();
            }
        }
        /// <summary>
        /// 获取验证码并自动识别
        /// </summary>
        /// <returns></returns>
        public string GetVerficar(string path)
        {
            var result = "";
            httpItem.URL = $"{urlbase}/verficar.do";
            httpItem.ResultType = ResultType.Byte;
            httpItem.Cookie = cookie;
            httpResult = httpHelper.GetHtml(httpItem);
            var image = byteArrayToImage(httpResult.ResultByte);
            image.Save(path + "code.bmp");

            cookie = httpResult?.Cookie.Replace("; Path=/erp", "").Trim();

            using (var engine = new TesseractEngine(path + "tessdata", "eng", EngineMode.Default))
            {
                engine.SetVariable("tessedit_char_whitelist", "0123456789");
                using (var pix = PixConverter.ToPix((Bitmap)image))
                {
                    using (var page = engine.Process(pix))
                    {
                        result = page.GetText();
                    }
                }
            }
            result = result.Replace("\n", "").Replace(" ", "").Trim();

            var timeSpan = (DateTime.Now.AddMilliseconds(expire * 1000) - DateTime.Now);
            RedisService.Instance.StringSet($"{RedisPrimaryKey.WebCrawlingCookie}/{uuid}", cookie, timeSpan);

            return result;
        }
        /// <summary>
        /// 自动登录
        /// </summary>
        /// <returns></returns>
        public string Login(string path = "")
        {
            if (string.IsNullOrEmpty(path))
                path = AppDomain.CurrentDomain.BaseDirectory + uploadFilePath;

            var loginresult = new LoginResult();
            var count = 0;

            ///获取验证码，重试maxCount次
            do
            {
                randomCode = GetVerficar(path);
                Thread.Sleep(500);
                count++;
            } while (randomCode.Length != 4 && count <= maxCount);

            if (string.IsNullOrEmpty(randomCode))
                return;

            var url = $"{urlbase}/checkLogin.do";
            var postData = $"softKeyId={softKeyId}&uuid={uuid}&uuidEncode={uuidEncode}&macStr={macStr}&loginMode={loginMode}&userNo={userNo}&userPass={userPass}&randomCode={randomCode}";
            var methodType = "POST";
            loginresult = Crawling<LoginResult>(url, postData, methodType);

            LoginStatus = loginresult?.loginStatus == 100;

            userinfo = loginresult?.userInfo;

            var timeSpan = (DateTime.Now.AddMilliseconds(expire * 1000) - DateTime.Now);
            RedisService.Instance.HashSet<Userinfo>(RedisPrimaryKey.WebCrawlingUserInfo, uuid, userinfo, timeSpan);

            return httpResult.Html;
        }
        /// <summary>
        /// 统一请求方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="methodType"></param>
        /// <returns></returns>
        public T Crawling<T>(string url, string postData = "", string methodType = "GET")
        {
            var next = true;
            var count = 0;
            do
            {
                httpItem.URL = url;
                httpItem.ContentType = "application/x-www-form-urlencoded";
                httpItem.ResultType = ResultType.String;
                httpItem.PostEncoding = Encoding.UTF8;
                httpItem.Cookie = cookie;
                httpItem.Method = methodType;
                httpItem.Postdata = postData;

                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.RedirectUrl.Contains("erp/login.do"))
                {
                    next = false;
                    Login();
                }
                count++;
            } while (!next && count < maxCount);

            var result = JSONHelper.Decode<T>(httpResult.Html);

            return result;
        }
        public string Crawling(string url, string postData = "", string methodType = "GET")
        {
            var next = true;
            var count = 0;
            do
            {
                httpItem.URL = url;
                httpItem.ContentType = "application/x-www-form-urlencoded";
                httpItem.ResultType = ResultType.String;
                httpItem.PostEncoding = Encoding.UTF8;
                httpItem.Cookie = cookie;
                httpItem.Method = methodType;
                httpItem.Postdata = postData;

                httpResult = httpHelper.GetHtml(httpItem);
                ///登录失效，重新登录
                if (httpResult.RedirectUrl.Contains("erp/login.do"))
                {
                    next = false;
                    Login();
                }
                count++;
            } while (!next && count < maxCount);

            return httpResult.Html;
        }
        /// <summary>
        /// 验证添加会员数据是否重复
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="shopid">店铺编号</param>
        /// <param name="vipCardNo">会员卡号</param>
        /// <returns>返回实体判断status=success即表示通过</returns>
        public vipCardNoAndMobileCheckResult vipCardNoAndMobileCheck(string mobile, string shopid, string vipCardNo = "")
        {
            var url = $"{urlbase}/customer/vipCardNoAndMobileCheck.do";
            var postData = $"vipCardNo={vipCardNo}&mobile={mobile}&mobileMust=true&vipCardNoMust=false&shopId={shopid}";
            var methodType = "POST";
            var result = Crawling<vipCardNoAndMobileCheckResult>(url, postData, methodType);
            return result;
        }
        /// <summary>
        /// 新增会员信息
        /// </summary>
        /// <param name="customerModel">会员信息</param>
        /// <returns>返回实体判断status=success即表示通过</returns>
        public vipCardNoAndMobileCheckResult saveCustomerBaseInfo(ZbErpCustomerModel customerModel)
        {
            var url = $"{urlbase}/customer/saveCustomerBaseInfo.do";
            var postData = $"integralDegree={customerModel.integralDegree}&cusName={customerModel.cusName}&mobile={customerModel.mobile}&vipCardNo={customerModel.vipCardNo}&vipCardLevelId={customerModel.vipCardLevelId}&vipCardLevel={customerModel.vipCardLevel}&sex={customerModel.sex}&birthdayType={customerModel.birthdayType}&birthday={customerModel.birthday}&sendDate={customerModel.sendDate}&shopId={customerModel.shopId}&shopName={customerModel.shopName}&identityCards={customerModel.identityCards}&qq={customerModel.qq}&purchaPreferences={customerModel.purchaPreferences}&hobby={customerModel.hobby}&profession={customerModel.profession}&province={customerModel.province}&city={customerModel.city}&area={customerModel.area}&address={customerModel.address}&anniversaries1Type={customerModel.anniversaries1Type}&anniversaries1Date={customerModel.anniversaries1Date}&anniversaries2Type={customerModel.anniversaries2Type}&anniversaries2Date={customerModel.anniversaries2Date}&anniversaries3Type={customerModel.anniversaries3Type}&anniversaries3Date={customerModel.anniversaries3Date}&agentId={customerModel.agentId}&agentName={customerModel.agentName}&referencesId={customerModel.referencesId}&referencesName={customerModel.referencesName}&source={customerModel.source}&remarks={customerModel.remarks}&pictureUrl={customerModel.pictureUrl}";

            var methodType = "POST";
            var result = Crawling<vipCardNoAndMobileCheckResult>(url, postData, methodType);
            return result;
        }
        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <param name="customerModel">会员信息</param>
        /// <returns>返回实体判断status=success即表示通过</returns>
        public vipCardNoAndMobileCheckResult updateCustomerBaseInfo(UpZbErpCustomerModel customerModel)
        {
            var url = $"{urlbase}/customer/updateCustomerBaseInfo.do";
            var postData = $"id={customerModel.id}&integralDegree={customerModel.integralDegree}&cusName={customerModel.cusName}&mobile={customerModel.mobile}&vipCardNo={customerModel.vipCardNo}&vipCardLevelId={customerModel.vipCardLevelId}&vipCardLevel={customerModel.vipCardLevel}&sex={customerModel.sex}&birthdayType={customerModel.birthdayType}&birthday={customerModel.birthday}&sendDate={customerModel.sendDate}&shopId={customerModel.shopId}&shopName={customerModel.shopName}&identityCards={customerModel.identityCards}&qq={customerModel.qq}&purchaPreferences={customerModel.purchaPreferences}&hobby={customerModel.hobby}&profession={customerModel.profession}&province={customerModel.province}&city={customerModel.city}&area={customerModel.area}&address={customerModel.address}&anniversaries1Type={customerModel.anniversaries1Type}&anniversaries1Date={customerModel.anniversaries1Date}&anniversaries2Type={customerModel.anniversaries2Type}&anniversaries2Date={customerModel.anniversaries2Date}&anniversaries3Type={customerModel.anniversaries3Type}&anniversaries3Date={customerModel.anniversaries3Date}&agentId={customerModel.agentId}&agentName={customerModel.agentName}&referencesId={customerModel.referencesId}&referencesName={customerModel.referencesName}&source={customerModel.source}&remarks={customerModel.remarks}&pictureUrl={customerModel.pictureUrl}";
            var methodType = "POST";
            var result = Crawling<vipCardNoAndMobileCheckResult>(url, postData, methodType);
            return result;
        }
        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <param name="customerModel">会员信息</param>
        /// <returns>返回实体判断status=success即表示通过</returns>
        public vipCardNoAndMobileCheckResult custPointAdjust(string customerId, int adjustType, int score)
        {
            var url = $"{urlbase}/customer/custPointAdjust.do";
            var postData = $"setType=0&custCount=1&custIds={customerId}&aimShopIds=&adjustBaseType1=1&adjustBaseValue1={(adjustType == 1 ? score : 0)}&adjustType={adjustType}&adjustBaseType2=1&adjustBaseValue2={(adjustType == 2 ? score : 0)}&adjustType3Date=&adjustRemarks=";
            var methodType = "POST";
            var result = Crawling<vipCardNoAndMobileCheckResult>(url, postData, methodType);
            return result;
        }
        /// <summary>
        /// 删除会员
        /// </summary>
        /// <param name="customerId">会员编号</param>
        /// <returns>返回实体判断status=success即表示通过</returns>
        public vipCardNoAndMobileCheckResult deleteCustomer(int customerId)
        {
            var url = $"{urlbase}/customer/deleteCustomer.do?id={customerId}";
            var postData = $"id={customerId}";
            var methodType = "POST";
            var result = Crawling<vipCardNoAndMobileCheckResult>(url, postData, methodType);
            return result.data;
        }
        /// <summary>
        /// 查询用户帐号数据
        /// </summary>
        /// <param name="start">从第N条开始取，默认从0条开始</param>
        /// <param name="length">取N条数据，默认50条</param>
        /// <param name="loginNo">登录用户名</param>
        /// <param name="realName">昵称</param>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        public List<UserDetail> loadUserList(int start = 0, int length = 50, string loginNo = "", string realName = "", string mobile = "")
        {
            var url = $"{urlbase}/user/loadUserList.do";
            var postData = $"draw=1&start={start}&length={length}&loginNo={loginNo}&realName={realName}&mobile={mobile}&locked=";
            var methodType = "POST";
            var result = Crawling<loadUserListResult>(url, postData, methodType);
            return result.data;
        }
        /// <summary>
        /// 获取门店员工数据
        /// </summary>
        /// <param name="shopId">门店编号</param>
        /// <returns></returns>
        public List<EmployeeDetailResult> getEmployeeListByShopId(int shopId)
        {
            var url = $"{urlbase}/employee/getEmployeeListByShopId.do?shopId={shopId}";
            var postData = $"";
            var methodType = "POST";
            var result = Crawling<List<EmployeeDetailResult>>(url, postData, methodType);
            return result.data;
        }

        /// <summary>
        /// 查找会员列表
        /// </summary>
        /// <param name="start">从第N条开始取，默认从0条开始</param>
        /// <param name="length">取N条数据，默认50条</param>
        /// <param name="keyword">可传入手机号、会员卡号、姓名、身份证</param>
        /// <returns></returns>
        public List<CustomerResult> loadCustomer(int start = 0, int length = 50, string keyword = "")
        {
            var url = $"{urlbase}/customer/loadCustomer.do";
            var postData = $"draw=1&columns[0][data]=&columns[0][name]=&columns[0][searchable]=true&columns[0][orderable]=false&columns[0][search][value]=&columns[0][search][regex]=false&columns[1][data]=&columns[1][name]=&columns[1][searchable]=true&columns[1][orderable]=false&columns[1][search][value]=&columns[1][search][regex]=false&columns[2][data]=&columns[2][name]=&columns[2][searchable]=true&columns[2][orderable]=false&columns[2][search][value]=&columns[2][search][regex]=false&columns[3][data]=shopName&columns[3][name]=&columns[3][searchable]=true&columns[3][orderable]=true&columns[3][search][value]=&columns[3][search][regex]=false&columns[4][data]=sendDate&columns[4][name]=&columns[4][searchable]=true&columns[4][orderable]=true&columns[4][search][value]=&columns[4][search][regex]=false&columns[5][data]=cusName&columns[5][name]=&columns[5][searchable]=true&columns[5][orderable]=true&columns[5][search][value]=&columns[5][search][regex]=false&columns[6][data]=vipCardLevel&columns[6][name]=&columns[6][searchable]=true&columns[6][orderable]=true&columns[6][search][value]=&columns[6][search][regex]=false&columns[7][data]=vipCardNo&columns[7][name]=&columns[7][searchable]=true&columns[7][orderable]=true&columns[7][search][value]=&columns[7][search][regex]=false&columns[8][data]=sex&columns[8][name]=&columns[8][searchable]=true&columns[8][orderable]=true&columns[8][search][value]=&columns[8][search][regex]=false&columns[9][data]=age&columns[9][name]=&columns[9][searchable]=true&columns[9][orderable]=true&columns[9][search][value]=&columns[9][search][regex]=false&columns[10][data]=mobile&columns[10][name]=&columns[10][searchable]=true&columns[10][orderable]=true&columns[10][search][value]=&columns[10][search][regex]=false&columns[11][data]=birthday&columns[11][name]=&columns[11][searchable]=true&columns[11][orderable]=true&columns[11][search][value]=&columns[11][search][regex]=false&columns[12][data]=birthdayType&columns[12][name]=&columns[12][searchable]=true&columns[12][orderable]=true&columns[12][search][value]=&columns[12][search][regex]=false&columns[13][data]=availablePoints&columns[13][name]=&columns[13][searchable]=true&columns[13][orderable]=true&columns[13][search][value]=&columns[13][search][regex]=false&columns[14][data]=consumpAmountTotal&columns[14][name]=&columns[14][searchable]=true&columns[14][orderable]=true&columns[14][search][value]=&columns[14][search][regex]=false&columns[15][data]=pointTotal&columns[15][name]=&columns[15][searchable]=true&columns[15][orderable]=true&columns[15][search][value]=&columns[15][search][regex]=false&columns[16][data]=businessTimeNum&columns[16][name]=&columns[16][searchable]=true&columns[16][orderable]=true&columns[16][search][value]=&columns[16][search][regex]=false&columns[17][data]=recentBusinessTime&columns[17][name]=&columns[17][searchable]=true&columns[17][orderable]=true&columns[17][search][value]=&columns[17][search][regex]=false&columns[18][data]=custStatus&columns[18][name]=&columns[18][searchable]=true&columns[18][orderable]=true&columns[18][search][value]=&columns[18][search][regex]=false&columns[19][data]=lossStatus&columns[19][name]=&columns[19][searchable]=true&columns[19][orderable]=true&columns[19][search][value]=&columns[19][search][regex]=false&columns[20][data]=&columns[20][name]=&columns[20][searchable]=true&columns[20][orderable]=true&columns[20][search][value]=&columns[20][search][regex]=false&columns[21][data]=&columns[21][name]=&columns[21][searchable]=true&columns[21][orderable]=true&columns[21][search][value]=&columns[21][search][regex]=false&columns[22][data]=profession&columns[22][name]=&columns[22][searchable]=true&columns[22][orderable]=true&columns[22][search][value]=&columns[22][search][regex]=false&columns[23][data]=&columns[23][name]=&columns[23][searchable]=true&columns[23][orderable]=true&columns[23][search][value]=&columns[23][search][regex]=false&columns[24][data]=address&columns[24][name]=&columns[24][searchable]=true&columns[24][orderable]=true&columns[24][search][value]=&columns[24][search][regex]=false&columns[25][data]=source&columns[25][name]=&columns[25][searchable]=true&columns[25][orderable]=true&columns[25][search][value]=&columns[25][search][regex]=false&columns[26][data]=referencesName&columns[26][name]=&columns[26][searchable]=true&columns[26][orderable]=true&columns[26][search][value]=&columns[26][search][regex]=false&columns[27][data]=remarks&columns[27][name]=&columns[27][searchable]=true&columns[27][orderable]=true&columns[27][search][value]=&columns[27][search][regex]=false&columns[28][data]=agentName&columns[28][name]=&columns[28][searchable]=true&columns[28][orderable]=true&columns[28][search][value]=&columns[28][search][regex]=false&order[0][column]=0&order[0][dir]=asc&start={start}&length={length}&search[value]=&search[regex]=false&notAreaFillter=true&keyword={keyword}";
            var methodType = "POST";
            var result = Crawling<LoadCustomerResult>(url, postData, methodType);
            return result.data;
        }

        /// <summary>
        /// 获取门店列表
        /// </summary>
        /// <returns></returns>
        public List<BasicShopDetail> loadBasicShopTree()
        {
            var url = $"{urlbase}/basicSetting/loadBasicShopTree.do";
            var postData = "";
            var methodType = "POST";
            var result = Crawling<List<BasicShopDetail>>(url, postData, methodType);
            return result;
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="sheetDateStart">必填，开始日期</param>
        /// <param name="sheetDateEnd">必填，结束日期</param>
        /// <param name="sheetNo">订单号</param>
        /// <param name="deptArea">门店</param>
        /// <param name="vipCardNo">客户卡号</param>
        /// <param name="cusName">顾客姓名</param>
        /// <param name="salesman1Names">导购1</param>
        /// <param name="salesman2Names">导购2</param>
        /// <param name="createUserNames">操作员</param>
        /// <param name="start">分页开始条数，如0-50，50-100</param>
        /// <param name="length">分页每页条数，如每页50条</param>
        /// <returns></returns>
        public LoadDetailDataResult loadDetailData(string sheetDateStart, string sheetDateEnd, string sheetNo, Dictionary<string, string> deptArea
            , string vipCardNo, string cusName, List<string> salesman1Names, List<string> salesman2Names, List<string> createUserNames
            , int start = 0, int length = 50)
        {
            var url = $"{urlbase}/reportV1/loadDetailData.do";
            var where = new StringBuilder();
            where.Append("[");
            where.Append("{\"f\":\"sheetDate\",\"o\":\"=\",\"v\":\"" + sheetDateStart + " to " + sheetDateEnd + "\",\"t\":\"dateTime\",\"l\":\"" + sheetDateStart + " to " + sheetDateEnd + "\"}");
            if (deptArea != null && deptArea.Count > 0)
                where.Append(",{\"f\":\"deptAreaCode\",\"o\":\"=\",\"v\":\"" + string.Join(",", deptArea.Select(t => t.Key)) + "\",\"t\":\"mutiSelect\",\"l\":\"" + string.Join(",", deptArea.Select(t => t.Value)) + "\"}");
            if (!string.IsNullOrEmpty(vipCardNo))
                where.Append(",{\"f\":\"vipCardNo\",\"o\":\"like\",\"v\":\"" + vipCardNo + "\",\"t\":\"input\",\"l\":\"" + vipCardNo + "\"}");
            if (!string.IsNullOrEmpty(cusName))
                where.Append(",{\"f\":\"cusName\",\"o\":\"like\",\"v\":\"" + cusName + "\",\"t\":\"input\",\"l\":\"" + cusName + "\"}");
            if (salesman1Names != null && salesman1Names.Count > 0)
                where.Append(",{\"f\":\"salesman1Name\",\"o\":\"=\",\"v\":\"" + string.Join(",", salesman1Names) + "\",\"t\":\"mutiSelect\",\"l\":\"" + string.Join(",", salesman1Names) + "\"}");
            if (salesman2Names != null && salesman2Names.Count > 0)
                where.Append(",{\"f\":\"salesman2Name\",\"o\":\"=\",\"v\":\"" + string.Join(",", salesman2Names) + "\",\"t\":\"mutiSelect\",\"l\":\"" + string.Join(",", salesman2Names) + "\"}");
            if (!string.IsNullOrEmpty(sheetNo))
                where.Append(",{\"f\":\"sheetNo\",\"o\":\"like\",\"v\":\"" + sheetNo + "\",\"t\":\"input\",\"l\":\"" + sheetNo + "\"},");
            if (createUserNames != null && createUserNames.Count > 0)
                where.Append(",{\"f\":\"createUserName\",\"o\":\"=\",\"v\":\"" + string.Join(",", createUserNames) + "\",\"t\":\"mutiSelect\",\"l\":\"" + string.Join(",", createUserNames) + "\"}");
            where.Append("]");
            var postData = "where=" + where.ToString() + "&start=" + start + "&length=" + length + "&id=12&order={}";
            var methodType = "POST";
            var result = Crawling<LoadDetailDataResult>(url, postData, methodType);
            return result;
        }

        /// <summary>
        /// 单个订单明细查询
        /// </summary>
        /// <param name="sheetId"></param>
        public List<OrderDetail> SaleDetail(int sheetId)
        {
            var url = $"{urlbase}/sale/createSaleSheet.do?canEditMainSheetSimpleInfo=1&sheetId=" + sheetId + "&h:702";
            var result = Crawling(url);
            return LoadOrderDetail(result);
        }
        /// <summary>
        /// 订单页面内容解析
        /// </summary>
        /// <param name="html"></param>
        public List<OrderDetail> LoadOrderDetail(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode sheetDateNode = doc.GetElementbyId("sheetDate");//text日期
            var sheetDate = sheetDateNode.Attributes["value"].Value;
            HtmlNode oldSheetNoNode = doc.GetElementbyId("oldSheetNo");//text单号
            var oldSheetNo = oldSheetNoNode.Attributes["value"].Value;
            HtmlNode remarksNode = doc.GetElementbyId("remarks");//text备注
            var remarks = remarksNode.Attributes["value"].Value;
            HtmlNode cusIdNode = doc.GetElementbyId("cusId");//姓名
            var cusId = cusIdNode.Attributes["value"].Value;
            HtmlNode cusNameNode = doc.GetElementbyId("cusName");//
            var cusName = cusNameNode.Attributes["value"].Value;
            HtmlNode mobileNode = doc.GetElementbyId("mobile");//电话
            var mobile = mobileNode.Attributes["value"].Value;
            HtmlNode vipCardNoNode = doc.GetElementbyId("vipCardNo");//卡号
            var vipCardNo = vipCardNoNode.Attributes["value"].Value;
            HtmlNode vipCardLevelNode = doc.GetElementbyId("vipCardLevel");//会员级别
            var vipCardLevel = vipCardLevelNode.Attributes["value"].Value;
            HtmlNode canUsePointNode = doc.GetElementbyId("canUsePoint");//可用积分
            var canUsePoint = canUsePointNode.Attributes["value"].Value;
            HtmlNode joinPointNode = doc.GetElementbyId("joinPoint");//checkbox本单参与积分
            var joinPoint = joinPointNode.Attributes["value"].Value;
            HtmlNode ponitNode = doc.GetElementbyId("ponit");//本次积分
            var ponit = ponitNode.Attributes["value"].Value;
            HtmlNode sheetTotalMoneyNode = doc.GetElementbyId("sheetTotalMoney");//本单金额
            var sheetTotalMoney = sheetTotalMoneyNode.Attributes["value"].Value;

            Regex reg = new Regex(@"var saleDataItemJson=(.+?);");
            var result = reg.Match(html).Groups;
            if (result.Count > 0)
            {
                var json = result[0].ToString().Replace("var saleDataItemJson=", "");
                json = json.Substring(0, json.Length - 1);
                return JSONHelper.Decode<List<OrderDetail>>(json);
            }
            return null;
        }
        /// <summary>
        /// 验证码识别
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        private Image byteArrayToImage(byte[] Bytes)
        {
            MemoryStream ms = new MemoryStream(Bytes);
            Image outputImg = Image.FromStream(ms);
            return outputImg;
        }
        /// <summary>
        /// 实体转换
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public UpZbErpCustomerModel ErpCustomerModelMap(CustomerResult customer)
        {
            var uperp = new UpZbErpCustomerModel();

            uperp.address = customer.address;
            uperp.agentId = customer.agentId;
            uperp.agentName = customer.agentName;
            uperp.anniversaries1Date = customer.anniversaries1Date;
            uperp.anniversaries1Type = customer.anniversaries1Type;
            uperp.anniversaries2Date = customer.anniversaries2Date;
            uperp.anniversaries2Type = customer.anniversaries2Type;
            uperp.anniversaries3Date = customer.anniversaries3Date;
            uperp.anniversaries3Type = customer.anniversaries3Type;
            uperp.area = customer.area;
            uperp.birthday = customer.birthday;
            uperp.birthdayType = customer.birthdayType;
            uperp.city = customer.city;
            uperp.cusName = customer.cusName;
            uperp.hobby = customer.hobby;
            uperp.id = customer.id;
            uperp.identityCards = customer.identityCards;
            uperp.integralDegree = customer.integralDegree;
            uperp.mobile = customer.mobile;
            uperp.pictureUrl = customer.pictureUrl;
            uperp.profession = customer.profession;
            uperp.province = customer.province;
            uperp.purchaPreferences = customer.purchaPreferences;
            uperp.qq = customer.qq;
            uperp.referencesId = customer.referencesId;
            uperp.referencesName = customer.referencesName;
            uperp.remarks = customer.remarks;
            uperp.sendDate = customer.sendDate;
            uperp.sex = customer.sex;
            uperp.shopId = customer.shopId;
            uperp.shopName = customer.shopName;
            uperp.source = customer.source;
            uperp.vipCardLevel = customer.vipCardLevel;
            uperp.vipCardLevelId = customer.vipCardLevelId;
            uperp.vipCardNo = customer.vipCardNo;

            return uperp;
        }
    }
}

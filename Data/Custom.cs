using CRM.Customer.Domain;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WJewel.Basic.Helper;

namespace CRM.Customer.Data
{
    /// <summary>
    /// 客户数据操作
    /// </summary>
    public class Custom
    {
        /// <summary>
        /// 判断客户号是否存在
        /// </summary>
        /// <param name="cusNo">客户号</param>
        /// <param name="mctNum">商户号</param>
        /// <returns></returns>
        public bool IsExistsCusNo(string cusNo, string mctNum)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.CusNo == cusNo && s.MctNum == mctNum).Any();
            }
        }
        /// <summary>
        /// 判断手机是否存在
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="storeId">门店编号</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsExistsPhone(string phone, string storeId)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.CusPhoneNo == phone && s.StoreId == storeId).Any();
            }
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="id">编号</param>
        /// <param name="status">状态值</param>
        /// <returns></returns>
        public bool ModifyStatus(string id, bool status)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var result = db.Ado.UseTran(() =>
                {
                    r = db.Updateable<Cus_Customer>()
                    .UpdateColumns(s => new Cus_Customer() { Status = status })
                    .Where(s => s.Id == id).ExecuteCommand() > 0;
                });
                return result.Data;
            }
        }
        /// <summary>
        /// 员工离职后更新跟进人信息
        /// </summary>
        /// <param name="staffId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public bool ModifyFollowStatus(string staffId, string storeId)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var result = db.Ado.UseTran(() =>
                {
                    r = db.Updateable<Cus_StaffCustomer>()
                    .UpdateColumns(s => new Cus_StaffCustomer() { StaffId = "", StaffName = "" })
                    .Where(s => s.StoreId == storeId && s.StoreId == staffId).ExecuteCommand() > 0;
                });
                return result.Data;
            }
        }

        /// <summary>
        /// 创建客户
        /// </summary>
        /// <returns></returns>
        public bool Create(List<Cus_Customer> create, List<Cus_CustomerTags> tags, List<Cus_IntegralRecord> integral, List<Cus_StaffCustomer> staffs, List<Cus_BaseCustomer> baseCsutom)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (create != null && create.Count > 0)
                    {
                        var obj = db.Insertable<Cus_Customer>(create).ExecuteCommand();
                    }
                    if (tags != null && tags.Count > 0)
                    {
                        var obj1 = db.Insertable<Cus_CustomerTags>(tags).ExecuteCommand();
                    }
                    if (integral != null && integral.Count > 0)
                    {
                        var obj2 = db.Insertable<Cus_IntegralRecord>(integral).ExecuteCommand();
                    }
                    if (staffs != null && staffs.Count > 0)
                    {
                        var obj3 = db.Insertable<Cus_StaffCustomer>(staffs).ExecuteCommand();
                    }
                    if (baseCsutom != null && baseCsutom.Count > 0)
                    {
                        var obj4 = db.Insertable<Cus_BaseCustomer>(baseCsutom).ExecuteCommand();
                    }
                });
                return result.Data;
            }
        }
        /// <summary>
        /// 删除客户
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool Delete(string[] ids)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var result = db.Ado.UseTran(() =>
                {
                    r = db.Updateable<Cus_Customer>()
                    .UpdateColumns(s => new Cus_Customer() { IsDelete = true })
                    .Where(s => ids.Contains(s.Id)).ExecuteCommand() > 0;
                });
                return result.Data;
            }
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="CartIds"></param>
        /// <returns></returns>
        public bool Update(List<Cus_Customer> model, List<Cus_CustomerTags> tags, List<Cus_StaffCustomer> staff, List<Cus_BaseCustomer> baseCsutom)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var result = db.Ado.UseTran(() =>
                {
                    if (model != null && model.Count > 0)
                    {
                        var obj = db.Updateable<Cus_Customer>(model).ExecuteCommand();
                    }

                    if (tags != null && tags.Count > 0)
                    {
                        var cusid = tags.FirstOrDefault().CusId;
                        var storeId = tags.FirstOrDefault().StoreId;
                        db.Deleteable<Cus_CustomerTags>().Where(o => o.CusId == cusid && o.StoreId == storeId).ExecuteCommand();
                        var obj1 = db.Insertable<Cus_CustomerTags>(tags).ExecuteCommand();
                    }
                    else
                    {
                        var cusid = model.FirstOrDefault().Id;
                        var storeId = model.FirstOrDefault().StoreId;
                        db.Deleteable<Cus_CustomerTags>().Where(o => o.CusId == cusid && o.StoreId == storeId).ExecuteCommand();
                    }

                    if (staff != null && staff.Count > 0)
                    {
                        var storeId = staff.FirstOrDefault().StoreId;
                        var cusId = staff.FirstOrDefault().CustomerId;
                        db.Deleteable<Cus_StaffCustomer>().Where(o => o.StoreId == storeId && o.CustomerId == cusId).ExecuteCommand();
                        var obj2 = db.Insertable<Cus_StaffCustomer>(staff).ExecuteCommand();
                    }
                    if (baseCsutom != null && baseCsutom.Count > 0)
                    {
                        var obj4 = db.Insertable<Cus_BaseCustomer>(baseCsutom).ExecuteCommand();
                    }
                });
                return result.Data;
            }
        }
        /// <summary>
        /// 根据商户号获取数据
        /// </summary>
        /// <param name="mctNum">商户号</param>
        /// <param name="totalCount">总条数</param>
        /// <param name="levelId">客户等级</param>
        /// <param name="IsOrdinaryCustom">是否普通客户等级</param>
        /// <param name="vague">客户名称/客户号 模糊搜索</param>
        /// <param name="storeIds">门店编号集合</param>
        /// <param name="currentStore">当前门店</param>
        /// <param name="cusName">客户名称</param>
        /// <param name="cusNo">客户号</param>
        /// <param name="phoneNo">手机号</param>
        /// <param name="cusSource">客户来源</param>
        /// <param name="cusFollowPerson">跟进人编号</param>
        /// <param name="status">状态</param>
        /// <param name="isActivation">是否激活</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Cus_Customer> GetList(string mctNum, ref int totalCount, string[] storeIds, string currentStore = "", string levelId = "", bool IsOrdinaryCustom = false, string vague = "", string cusName = "",
            string cusNo = "", string phoneNo = "", string cusSource = "", string cusFollowPerson = "", int status = -1, int isActivation = -1, int pageIndex = 1, int pageSize = 15)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var boolStatus = status == 0 ? false : true;
                var boolIsActivation = isActivation == 0 ? false : true;
                var entityList = db.Queryable<Cus_Customer, Cus_StaffCustomer>((s, s1) => new object[] { JoinType.Left, s.Id == s1.CustomerId && s1.StoreId == currentStore })
                    .Where(s => s.MctNum == mctNum && s.IsDelete == false)
                    .WhereIF(!IsOrdinaryCustom && !string.IsNullOrWhiteSpace(levelId), s => s.CusLevelId == levelId)
                    .WhereIF(IsOrdinaryCustom && !string.IsNullOrWhiteSpace(levelId), s => s.CusLevelId == levelId || SqlFunc.IsNullOrEmpty(s.CusLevelId))
                    .WhereIF(!string.IsNullOrWhiteSpace(vague), s => s.CusName.Contains(vague) || s.CusNo.Contains(vague))
                    .WhereIF(!string.IsNullOrWhiteSpace(cusName), s => s.CusName.Contains(cusName))
                    .WhereIF(!string.IsNullOrWhiteSpace(cusNo), s => s.CusNo.Contains(cusNo))
                    .WhereIF(!string.IsNullOrWhiteSpace(phoneNo), s => s.CusPhoneNo.Contains(phoneNo))
                    .WhereIF(!string.IsNullOrWhiteSpace(cusSource), s => s.CusSource == cusSource)
                    .WhereIF(!string.IsNullOrWhiteSpace(cusFollowPerson) && cusFollowPerson != "0", (s, s1) => s1.StaffId == cusFollowPerson)
                    .WhereIF(!string.IsNullOrWhiteSpace(cusFollowPerson) && cusFollowPerson == "0", (s, s1) => SqlFunc.IsNullOrEmpty(s1.StaffId))
                    .WhereIF(new int[] { 0, 1 }.Contains(status), s => s.Status == boolStatus)
                    .WhereIF(new int[] { 0, 1 }.Contains(isActivation), s => s.IsActivation == boolIsActivation)
                    .WhereIF(storeIds != null && storeIds.Count() > 0, s => storeIds.Contains(s.StoreId));

                return entityList.OrderBy(s => s.LastModifyTime, OrderByType.Desc)
                    .ToPageList(pageIndex, pageSize, ref totalCount);
            }
        }

        /// <summary>
        /// 根据客户id获取客户列表
        /// </summary>
        /// <param name="ids">客户id列表</param>
        /// <returns></returns>
        public List<Cus_Customer> GetListByIds(string[] ids)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var entityList = db.Queryable<Cus_Customer>()
                    .Where(s => s.IsDelete == false && s.Status)
                    .WhereIF(ids != null && ids.Count() > 0, s => ids.Contains(s.Id));

                return entityList.OrderBy(s => s.LastModifyTime, OrderByType.Desc).ToList();
            }
        }

        public List<V_Cus_Customer> GetCusList(ref int totalCount, string mctNum, string[] storeIds, string currentStore = "", string levelId = "", bool IsOrdinaryCustom = false, string vague = "", string cusName = "",
            string cusNo = "", string phoneNo = "", string cusSource = "", string cusFollowPerson = "", int status = -1, int isActivation = -1
            , string startConsume = "", string endConsume = "", string startReg = "", string endReg = ""
            , string SortColumn = "s.CusRegisterTime", string SortType = "DESC"
            , int pageIndex = 1, int pageSize = 15)
        {
            var sqlselect = @" SELECT  s.CusAccumulatedPoints, s.CusBirthday, s.CusCurrentScore, s.CusLevelId, s.Id, s.CusLogo, s.CusName, s.CusNo, s.CusPhoneNo, s.CusRemark, s.CusSex, s.CusWechatNo, s.IsActivation, s.Status, s.CusIsBindWX, s.CusInitialIntegral, s.StoreId, s.CusRegisterTime, s2.CreatedDate, s.MctNum, s3.BirthdayConsumption, s3.Grade AS CusLevelName, s3.OrdinaryConsumption, s3.OtherEquity, s3.ProductOffer, s2.CreatedDate AS LastConsumeTime";
            var sqlform = @" FROM Cus_Customer AS s
                                LEFT JOIN Cus_StaffCustomer AS s1 ON s.Id = s1.CustomerId AND s1.StoreId = s.StoreId
                                LEFT JOIN (
	                                SELECT
		                                a.CustomerId,
		                                MAX(a.CreatedDate) AS CreatedDate
	                                FROM
		                                Cus_IntegralRecord AS a	
	                                WHERE a.IntegralType = '新增消费'
                                    GROUP BY a.CustomerId
                                    ) AS s2 ON s.Id= s2.CustomerId
                                LEFT JOIN Cus_CustomerLevel AS s3 ON s.CusLevelId = s3.Id";

            var sqlorderby = $" ORDER BY {SortColumn} {SortType} ";

            var where = new StringBuilder();
            where.Append($" WHERE s.MctNum = '{mctNum}' and s.IsDelete=false ");
            if (!IsOrdinaryCustom && !string.IsNullOrWhiteSpace(levelId))
                where.Append($" AND s.CusLevelId = '{levelId}'");
            if (IsOrdinaryCustom && !string.IsNullOrWhiteSpace(levelId))
                where.Append($" AND (s.CusLevelId = '{levelId}' OR s.CusLevelId IS NULL OR s.CusLevelId = '')");
            if (!string.IsNullOrWhiteSpace(vague))
                where.Append($" AND (s.CusName LIKE '%{vague}%' || s.CusNo  LIKE '%{vague}%')");
            if (!string.IsNullOrWhiteSpace(cusName))
                where.Append($" AND (s.CusName LIKE '%{cusName}%')");
            if (!string.IsNullOrWhiteSpace(cusNo))
                where.Append($" AND (s.CusNo LIKE '%{cusName}%')");
            if (!string.IsNullOrWhiteSpace(phoneNo))
                where.Append($" AND (s.CusPhoneNo LIKE '%{phoneNo}%')");
            if (!string.IsNullOrWhiteSpace(cusSource))
                where.Append($" AND s.CusSource = '{cusSource}'");
            if (!string.IsNullOrWhiteSpace(cusFollowPerson) && cusFollowPerson != "0")
                where.Append($" AND s1.StaffId = '{cusFollowPerson}'");
            if (!string.IsNullOrWhiteSpace(cusFollowPerson) && cusFollowPerson == "0")
                where.Append($" AND (s1.StaffId IS NULL OR s1.StaffId = '')");
            if (new int[] { 0, 1 }.Contains(status))
                where.Append($" AND s.Status = {status}");
            if (new int[] { 0, 1 }.Contains(isActivation))
                where.Append($" AND s.IsActivation = {isActivation}");
            if (storeIds != null && storeIds.Count() > 0)
                where.Append($" AND find_in_set(s.StoreId,'{string.Join(",", storeIds)}')");

            if (!string.IsNullOrEmpty(startConsume))
                where.Append($" AND s2.CreatedDate >= '{startConsume} 00:00:00'");
            if (!string.IsNullOrEmpty(endConsume))
                where.Append($" AND s2.CreatedDate <= '{endConsume} 23:59:59'");
            if (!string.IsNullOrEmpty(startReg))
                where.Append($" AND s.CusRegisterTime >= '{startReg} 00:00:00'");
            if (!string.IsNullOrEmpty(endReg))
                where.Append($" AND s.CusRegisterTime <= '{endReg} 23:59:59'");

            var limit = $" LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var sql = sqlselect + sqlform + where.ToString() + sqlorderby;

                var count = db.Ado.SqlQuery<int>(sqlselect + sqlform + where.ToString());
                totalCount = count.Count();
                var list = db.Ado.SqlQuery<V_Cus_Customer>(sql + limit).ToList();

                return list;
            }
        }

        /// <summary>
        /// 根据编号集合获取数据
        /// </summary>
        /// <param name="mctNum">商户号</param>
        /// <param name="ids">总条数</param>
        /// <returns></returns>
        public List<Cus_Customer> GetList(string mctNum, string[] storeIds, string[] ids)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.IsDelete == false)
                    .WhereIF(!string.IsNullOrWhiteSpace(mctNum), s => s.MctNum == mctNum)
                    .WhereIF(storeIds != null && storeIds.Count() > 0, s => storeIds.Contains(s.StoreId))
                    .WhereIF(ids != null && ids.Count() > 0, s => ids.Contains(s.Id))
                    .ToList();
            }
        }

        /// <summary>
        /// 根据编号获取数据
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public Cus_Customer GetById(string id)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.Id == id)
                    .First();
            }
        }

        /// <summary>
        /// 根据编号获取数据
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public Cus_Customer GetByFaceId(int faceId, string mctNum, string[] storeIds)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.CusFaceId == faceId && s.MctNum == mctNum && storeIds.Contains(s.StoreId) && s.IsDelete == false)
                    .First();
            }
        }

        /// <summary>
        /// 根据人脸编号获取数据
        /// </summary>
        /// <param name="faceIds"></param>
        /// <returns></returns>
        public List<Cus_Customer> GetListByFaceIds(int[] faceIds)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => faceIds.Contains(s.CusFaceId) && s.IsDelete == false)
                    .ToList();
            }
        }


        /// <summary>
        /// 根据等级获取数据
        /// </summary>
        /// <param name="levelIds"></param>
        /// <param name="mctNum"></param>
        /// <returns></returns>
        public List<Cus_Customer> GetListByLevelIds(string mctNum, string[] storeIds, string[] levelIds)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.MctNum == mctNum && storeIds.Contains(s.StoreId) && levelIds.Contains(s.CusLevelId) && s.IsDelete == false)
                    .ToList();
            }
        }

        /// <summary>
        /// 根据标签获取数据
        /// </summary>
        /// <param name="labelIds"></param>
        /// <param name="mctNum"></param>
        /// <returns></returns>
        public List<Cus_Customer> GetListByLabelIds(string mctNum, string[] storeIds, string[] labelIds)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.MctNum == mctNum && storeIds.Contains(s.StoreId) && labelIds.Contains(s.CusLevelId) && s.IsDelete == false)
                    .ToList();
            }
        }

        /// <summary>
        /// 获取有人脸信息的客户
        /// </summary>
        /// <param name="mctNum"></param>
        /// <returns></returns>
        public List<Cus_Customer> GetHaveFaceIdList(string mctNum, string[] storeIds)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.MctNum == mctNum && storeIds.Contains(s.StoreId) && s.CusFaceId != 0 && s.IsDelete == false)
                    .ToList();
            }
        }
        /// <summary>
        /// 根据客户手机号获取数据
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public Cus_Customer GetByMctAndPhone(string mctNum, string phone, string storeId)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(s => s.MctNum == mctNum)
                    .Where(s => s.CusPhoneNo == phone && s.IsDelete == false)
                    .WhereIF(!string.IsNullOrWhiteSpace(storeId), s => s.StoreId == storeId)
                    .OrderBy(s => s.CusCurrentScore, OrderByType.Desc)
                    .First();
            }
        }
        /// <summary>
        /// 获取权重大于卡卷发放指定的客户等级
        /// </summary>
        /// <param name="storeIds"></param>
        /// <param name="levelIds">客户编号</param>
        /// <param name="IsOrdinaryMember">LevelIds是否存在普通会员等级</param>
        /// <returns></returns>
        public List<Cus_Customer> GetList(string[] storeIds, string[] levelIds, bool IsOrdinaryMember)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer, Cus_CustomerLevel>((s1, s2) => new object[] { JoinType.Left, s1.CusLevelId == s2.Id })
                    .Where(s1 => s1.Status == true && s1.IsDelete == false)
                    .WhereIF(storeIds != null && storeIds.Count() > 0, s1 => storeIds.Contains(s1.StoreId))
                    .WhereIF(levelIds != null && levelIds.Count() > 0 && !IsOrdinaryMember, (s1, s2) => levelIds.Contains(s2.Id))
                    .WhereIF(levelIds != null && levelIds.Count() > 0 && IsOrdinaryMember, (s1, s2) => levelIds.Contains(s2.Id) || SqlFunc.IsNullOrEmpty(s2.Id))
                    .ToList();
            }
        }

        /// <summary>
        /// 根据客户积分获取符合的门店列表
        /// </summary>
        /// <param name="integral"></param>
        /// <returns></returns>
        public List<Cus_Customer> GetStoreListByIntegral(string mctNum, int integral, string phoneNo)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(o => o.MctNum == mctNum && o.IsDelete == false && o.Status == true && o.CusCurrentScore >= integral && o.CusPhoneNo == phoneNo)
                    .ToList();
            }
        }

        /// <summary>
        /// 根据客户积分获取符合的门店列表
        /// </summary>
        /// <param name="integral"></param>
        /// <returns></returns>
        public List<Cus_Customer> GetStoreListByPhoneNo(string mctNum, string phoneNo)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer>()
                    .Where(o => o.MctNum == mctNum && o.IsDelete == false && o.Status == true && o.CusPhoneNo == phoneNo)
                    .ToList();
            }
        }


        /// <summary>
        /// 根据客户编号获取客户标签
        /// </summary>
        /// <param name="integral"></param>
        /// <returns></returns>
        public List<Cus_CustomerTags> GetCustomerTags(string[] cusIds)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_CustomerTags>()
                    .Where(o => cusIds.Contains(o.CusId))
                    .ToList();
            }
        }
        /// <summary>
        /// 根据门店、日期按等级获取客户汇总
        /// </summary>
        /// <param name="mctNum"></param>
        /// <param name="storeIds"></param>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <param name="isActivation"></param>
        /// <returns></returns>
        public Dictionary<string, int> StatisticsList(string mctNum, string storeIds, string dateStart, string dateEnd, bool isActivation = false)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var query = db.Queryable<Cus_Customer, Cus_CustomerLevel>((s, sl) => new object[] { JoinType.Left, s.CusLevelId == sl.Id })
                    .Where((s, sl) => s.MctNum == mctNum)
                    .WhereIF(!string.IsNullOrEmpty(storeIds), (s, sl) => storeIds.Contains(s.StoreId))
                    .WhereIF(isActivation, (s, sl) => s.IsActivation == isActivation)
                    .WhereIF(!string.IsNullOrWhiteSpace(dateStart) && !string.IsNullOrWhiteSpace(dateEnd), (s, sl) => s.CreateTime >= Convert.ToDateTime($"{dateStart} 00:00:00") && s.CreateTime <= Convert.ToDateTime($"{dateEnd} 23:59:59"))
                    .GroupBy((s, sl) => new { Grade = sl.Grade })
                    .Select((s, sl) => new { Grade = sl.Grade, Count = SqlFunc.AggregateCount(1) });
                ;
                var data = query.ToList();
                var addCustomer = new Dictionary<string, int>();
                if (data != null)
                    foreach (var item in data)
                    {
                        if (!string.IsNullOrEmpty(item.Grade))
                            addCustomer.Add(item.Grade, item.Count);
                    }

                return addCustomer;
            }
        }
        /// <summary>
        /// 根据门店按等级获取客户汇总
        /// </summary>
        /// <param name="mctNum"></param>
        /// <param name="storeIds"></param>
        /// <param name="isActivation"></param>
        /// <returns></returns>
        public Dictionary<string, int> StatisticsList(string mctNum, string storeIds, bool isActivation = false)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var query = db.Queryable<Cus_Customer, Cus_CustomerLevel>((s, sl) => new object[] { JoinType.Left, s.CusLevelId == sl.Id })
                    .Where((s, sl) => s.MctNum == mctNum)
                    .WhereIF(!string.IsNullOrEmpty(storeIds), (s, sl) => storeIds.Contains(s.StoreId))
                    .WhereIF(isActivation, (s, sl) => s.IsActivation == isActivation)
                    .GroupBy((s, sl) => new { Grade = sl.Grade })
                    .Select((s, sl) => new { Grade = sl.Grade, Count = SqlFunc.AggregateCount(1) });
                ;
                var data = query.ToList();
                var addCustomer = new Dictionary<string, int>();
                if (data != null)
                    foreach (var item in data)
                    {
                        if (!string.IsNullOrEmpty(item.Grade))
                            addCustomer.Add(item.Grade, item.Count);
                    }

                return addCustomer;
            }
        }
        /// <summary>
        /// 查询固定时间内注册的会员在固定时间内的交易会员列表
        /// </summary>
        /// <param name="mctNum"></param>
        /// <param name="storeIds"></param>
        /// <param name="regStart">注册时间</param>
        /// <param name="regEnd">注册时间</param>
        /// <param name="dateStart">交易时间</param>
        /// <param name="dateEnd">交易时间</param>
        /// <returns>返回Dictionary，Key为注册会员编号，Value为交易会员编号，Value为空则表示查询交易时间内无记录</returns>
        public List<VZombieCustom> ZombieCustomerList(string mctNum, string storeIds, string regStart, string regEnd, string dateStart, string dateEnd)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var query = db.Queryable<Cus_CustomerLevel, Cus_Customer, Cus_IntegralRecord>((l, s, sl) => new object[] { JoinType.Left, s.CusLevelId == l.Id, JoinType.Left, s.Id == sl.CustomerId })
                    .Where((l, s, sl) => s.MctNum == mctNum)
                    .Where((l, s, sl) => s.CreateTime >= Convert.ToDateTime($"{regStart} 00:00:00") && s.CreateTime <= Convert.ToDateTime($"{regEnd} 23:59:59"))
                    .Where((l, s, sl) => sl.CreatedDate >= Convert.ToDateTime($"{dateStart} 00:00:00") && sl.CreatedDate <= Convert.ToDateTime($"{dateEnd} 23:59:59"))
                    .WhereIF(!string.IsNullOrEmpty(storeIds), (l, s, sl) => storeIds.Contains(s.StoreId))
                    .GroupBy((l, s, sl) => new { l.Id, l.Grade, CustomerId = s.Id, ActiveCustomerId = sl.CustomerId })
                    .OrderBy((l, s, sl) => l.Weights)
                    .Select((l, s, sl) => new VZombieCustom { GradeId = l.Id, GradeName = l.Grade, CustomerId = s.Id, ActiveCustomerId = sl.CustomerId });
                ;
                return query.ToList();
            }
        }
        /// <summary>
        /// 根据商户号获取数据
        /// </summary>
        /// <param name="mctNum">商户号</param>
        /// <param name="storeIds">门店编号集合</param>
        /// <param name="cusId"></param>
        /// <returns></returns>
        public List<Cus_Customer> GetListByCusId(string mctNum, string storeIds, string[] cusId)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer, Cus_StaffCustomer>((s, s1) => new object[] { JoinType.Left, s.Id == s1.CustomerId })
                    .Where((s, s1) => s.MctNum == mctNum && s.IsDelete == false && storeIds.Contains(s.StoreId))
                    .Where((s, s1) => SqlFunc.ContainsArray(cusId, s.Id))
                    .ToList();
            }
        }


        /// <summary>
        /// 根据跟进人id获取客户列表
        /// </summary>
        /// <param name="staffId">跟进人id</param>
        /// <returns></returns>
        public List<Cus_Customer> GetListByStaffId(string staffId)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                var entityList = db.Queryable<Cus_Customer>()
                    .Where(s => s.CusFollowPerson == staffId);
                return entityList.OrderBy(s => s.LastModifyTime, OrderByType.Desc)
                    .ToList();
            }
        }

        /// <summary>
        /// 根据销售日期获取数据
        /// </summary>
        /// <param name="mctNum">商户号</param>
        /// <param name="storeIds">门店编号</param>
        /// <param name="levelIds">等级</param>
        /// <param name="labelIds">标签</param>
        /// <param name="ids">编号</param>
        /// <param name="beginSaleDate">销售日期</param>
        /// <param name="endSaleDate"></param>
        /// <returns></returns>
        public List<Cus_Customer> SearchForMessage(string mctNum, string[] storeIds, string[] levelIds, string[] labelIds, string[] ids, string beginSaleDate, string endSaleDate)
        {
            using (SqlSugarClient db = MySqlHelper.GetInstance())
            {
                return db.Queryable<Cus_Customer, Cus_QualityPolicy, Cus_CustomerTags>((s, s1, s2) => new object[] { JoinType.Left, s.Id == s1.CustomerId, JoinType.Left, s.Id == s2.CusId })
                    .Where((s, s1, s2) => !s.IsDelete && s.IsActivation)
                    .WhereIF(!string.IsNullOrWhiteSpace(mctNum), (s, s1, s2) => s.MctNum == mctNum)
                    .WhereIF(levelIds != null && levelIds.Count() > 0, s => levelIds.Contains(s.CusLevelId))
                    .WhereIF(labelIds != null && labelIds.Count() > 0, (s, s1, s2) => labelIds.Contains(s2.LabelId))
                    .WhereIF(ids != null && ids.Count() > 0, s => ids.Contains(s.Id))
                    .WhereIF(storeIds != null && storeIds.Count() > 0, s => storeIds.Contains(s.StoreId))
                    .WhereIF(!string.IsNullOrWhiteSpace(beginSaleDate), (s, s1, s2) => s1.CreateTime >= SqlFunc.ToDate($"{beginSaleDate} 00:00:00"))
                    .WhereIF(!string.IsNullOrWhiteSpace(endSaleDate), (s, s1, s2) => s1.CreateTime <= SqlFunc.ToDate($"{endSaleDate} 23:59:59"))
                    .OrderBy((s, s1, s2) => s.LastModifyTime, OrderByType.Desc)
                    .ToList();
            }
        }
    }
}
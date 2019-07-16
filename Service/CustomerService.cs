using CRM.Customer.Data;
using CRM.Customer.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WJewel.Basic;
using WJewel.DataContract.Common;
using WJewel.DataContract.CRM.Common;
using WJewel.DataContract.CRM.Customer;
using WJewel.DataContract.CRM.Report;
using WJewel.DataContract.CRM.Enum;
using WJewel.DataContract.CRM.System;
using WJewel.DataContract.CRM.Wisdom;
using WJewel.DataContract.WJewel.Common;
using WJewel.ServicePull.CRM.Customer;
using WJewel.ServicePull.CRM.Platform;
using WJewel.ServicePull.CRM.System;
using WJewel.ServicePull.CRM.Wisdom;
using WJewel.ServicePull.UserCenter;
using WJewel.ServicePull.WJewel.Config;
using WJewel.ServicePull.WJewel.Order;
using WJewel.ServicePull.WJewel.UserCenter;
using System.Web;
using System.Threading;

namespace CRM.Customer.Service
{
    public class CustomerService
    {
        /// <summary>
        /// 平台商户号
        /// </summary>
        public static string PlatMctNum = ConfigHelper.GetAppSetting("admin");
        /// <summary>
        /// 客户页面初始化
        /// </summary>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult<CustomerInit> PageInit(UserData ud)
        {

            OperatResult<CustomerInit> res = new OperatResult<CustomerInit>();
            List<string> storeIds = new List<string>();
            var chief = StoreAndStaffServiceTransfer.GetChiefByMct(ud.MctNum);
            if (chief != null)
            {
                storeIds.Add(ud.StoreId);
                if (chief.Id != ud.StoreId)
                {
                    storeIds.Add(chief.Id);
                }
            }
            else
            {
                storeIds.Add(chief.Id);
            }
            var resLabelList = new CustomerLabel().GetList(ud.MctNum, storeIds.ToArray());
            List<DropDownLabel> resLabel = new List<DropDownLabel>();
            if (resLabelList != null && resLabelList.Count > 0)
            {
                foreach (var item in resLabelList.OrderByDescending(o => o.Stick))
                {
                    DropDownLabel drop = new DropDownLabel()
                    {
                        Id = item.Id,
                        Name = item.LabelName,
                        Status = item.Status,
                    };
                    resLabel.Add(drop);
                }
            }
            if (resLabel == null)
                resLabel = new List<DropDownLabel>();
            var resLevel = CustomerLevelService.GetDropDownList(ud);
            if (resLevel == null)
                resLevel = new OperatResult<List<DropDownModel>>();
            else
            {
                if (resLevel.ReturnData != null && resLevel.ReturnData.Count > 0)
                {
                    var level = resLevel.ReturnData.Find(o => o.Name == "游客");
                    if (level != null)
                    {
                        resLevel.ReturnData.Remove(level);
                    }
                }
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<DropDownModel> sourceList = new List<DropDownModel>();
            var resSource = DictionaryServiceTransfer.GetDictionaryValueListNoToken(CRMDictionaryModel.CusSource, "");
            if (resSource != null)
            {
                foreach (var item in resSource)
                {
                    DropDownModel drop = new DropDownModel()
                    {
                        Id = item.Id,
                        Name = item.ValueName,
                    };
                    sourceList.Add(drop);
                }
            }
            sw.Stop();

            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            List<DropDownModel> staffList = new List<DropDownModel>();
            var resStaff = StoreAndStaffServiceTransfer.GetDropList(ud.TokenStr);
            if (staffList != null)
            {
                foreach (var item in resStaff)
                {
                    DropDownModel drop = new DropDownModel()
                    {
                        Id = item.Id,
                        Name = item.StaffName,
                    };
                    staffList.Add(drop);
                }
            }
            sw1.Stop();

            CustomerInit init = new CustomerInit()
            {
                LabelList = resLabel,
                LebelList = resLevel.ReturnData,
                SourceList = sourceList,
                StaffList = staffList,
            };
            res.ReturnData = init;
            res.Success = true;
            res.Message = "";
            return res;
        }

        //<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        private static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }

        /// <summary>
        /// 获取客户号
        /// </summary>
        /// <param name="mctFlag"></param>
        /// <param name="mctNum"></param>
        /// <returns></returns>
        public static string GetCusNo(string mctFlag, string mctNum)
        {
            return "";
            var cusNo = "";
            var next = true;
            do
            {
                cusNo = GetRandomString(10, true, true, true, false, mctFlag);
                if (new Custom().IsExistsCusNo(cusNo, mctNum))
                {
                    next = false;
                }
            } while (!next);
            return cusNo;
        }

        /// <summary>
        /// 获取距离明天零时零分零秒多少秒
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        private static TimeSpan GetTomorrowZeroTime(DateTime datetime)
        {
            TimeSpan timespan = new TimeSpan(0, 0, 30, 0);
            DateTime yesdt = datetime.Add(timespan);
            return new DateTime(yesdt.Year, yesdt.Month, yesdt.Day, yesdt.Hour, yesdt.Minute, yesdt.Second) - datetime;
        }

        /// <summary>
        /// 创建客户
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult<string> Create(CreateCustomer model, UserData ud)
        {
            OperatResult<string> res = new OperatResult<string>();
            StaffDetailInfoModel staffModel = null;
            var customData = new Custom();
            if (!string.IsNullOrWhiteSpace(model.CusFollowPerson))
            {
                staffModel = StoreAndStaffServiceTransfer.GetStaffDetailInfo(model.CusFollowPerson);
                if (staffModel == null)
                {
                    res.Success = false;
                    res.Message = "未找到跟进人的员工信息！";
                    return res;
                }
            }
            var tempLevelName = "";
            var score = 0M;
            var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
            var scoreLevel = GetCustomerLevel((decimal)model.CusCurrentScore, ud.MctNum);
            if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
            {
                var levelList = levelPageList.ReturnData.DataList;
                if (!string.IsNullOrWhiteSpace(model.CusLevelId))
                {
                    var level = levelList.Find(o => o.Id == model.CusLevelId);
                    if (level == null)
                    {
                        var tempLevel = scoreLevel != null && scoreLevel.ReturnData != null ? scoreLevel.ReturnData : levelList.Find(o => o.Grade == "普通客户");
                        model.CusLevelId = tempLevel != null ? tempLevel.Id : "";
                        tempLevelName = tempLevel != null ? tempLevel.Grade : "";
                        score = (decimal)(tempLevel != null ? tempLevel.AchieveConditions : 0);
                    }
                    else
                    {
                        tempLevelName = level != null ? level.Grade : "";
                        score = (decimal)(level != null ? level.AchieveConditions : 0);
                        model.CusLevelId = level.Id;
                    }
                }
                else
                {
                    var tempLevel = scoreLevel != null && scoreLevel.ReturnData != null ? scoreLevel.ReturnData : levelList.Find(o => o.Grade == "普通客户");
                    model.CusLevelId = tempLevel != null ? tempLevel.Id : "";
                    tempLevelName = tempLevel != null ? tempLevel.Grade : "";
                    score = (decimal)(tempLevel != null ? tempLevel.AchieveConditions : 0);
                }
            }
            else
            {
                res.Success = false;
                res.Message = "未找到客户等级信息！";
                return res;
            }
            if (scoreLevel != null && scoreLevel.ReturnData != null)
            {
                if (scoreLevel.ReturnData.Id != model.CusLevelId)
                {
                    var message = "";
                    if (score > scoreLevel.ReturnData.AchieveConditions)
                    {
                        message = "该客户初始积分匹配的会员等级为【" + scoreLevel.ReturnData.Grade + "】,请调整会员等级。";
                    }
                    else if (score < scoreLevel.ReturnData.AchieveConditions)
                    {
                        message = "该客户初始积分匹配的会员等级为【" + scoreLevel.ReturnData.Grade + "】,请调整会员等级。";
                    }
                    res.Success = false;
                    res.Message = message;
                    return res;
                }
            }
            if (!string.IsNullOrWhiteSpace(model.CusSource))
            {
                var cusSource = DictionaryServiceTransfer.GetDictionaryValueListNoToken(CRMDictionaryModel.CusSource, "");
                if (cusSource == null)
                {
                    res.Success = false;
                    res.Message = "未找到客户来源信息！";
                    return res;
                }
                var source = cusSource.Find(o => o.ValueName == model.CusSource);
                if (source == null)
                {
                    res.Success = false;
                    res.Message = "客户来源不在字典中！";
                    return res;
                }
            }

            List<string> storeIds = new List<string>();
            var storeList = StoreAndStaffServiceTransfer.GetStoreList(ud.TokenStr); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
            if (storeList != null && storeList.Count > 0)
            {
                var store = storeList.Find(o => o.Id == ud.StoreId);
                if (store == null)
                {
                    storeIds.Add(ud.StoreId); //改组没有查询到自身信息，直接返回自身
                }
                else
                {
                    if (store.IsShareMemberAIntegral)
                    {
                        foreach (var item in storeList)
                        {
                            storeIds.Add(item.Id);
                        }
                    }
                    else
                    {
                        storeIds.Add(ud.StoreId); //门店组没有共享会员信息
                    }
                }
            }
            else
            {
                storeIds.Add(ud.StoreId); //没有查询到门店组信息，直接返回自身
            }
            var custom = customData.GetByPhone(ud.MctNum, model.CusPhoneNo, storeIds.ToArray());
            if (custom != null)
            {
                if (custom.StoreId == ud.StoreId)
                {
                    res.Success = false;
                    res.Message = "客户已存在！";
                    return res;
                }
                else
                {
                    res.Success = false;
                    res.Message = "客户已存在共享！";
                    return res;
                }
            }
            if (model.CusFaceId > 0)
            {
                var cus = customData.GetByFaceId(model.CusFaceId, ud.MctNum, storeIds.ToArray());
                if (cus != null)
                {
                    res.Success = false;
                    res.Message = "客户号为：【" + cus.CusNo + "】的客户已绑定了此人脸信息！";
                    return res;
                }
            }
            DateTime? birthday = null;
            if (model.CusBirthday != null)
            {
                birthday = Convert.ToDateTime(model.CusBirthday).Date;
            }

            string mctFlag = UserCenterServiceTransfer.GetMctCode(ud.TokenStr);
            Cus_Customer create = new Cus_Customer()
            {
                CreateAccount = ud.Account,
                CreateTime = DateTime.Now,
                CreateUserId = ud.UserID,
                CusAccumulatedPoints = model.CusCurrentScore,
                CusAddress = model.CusAddress,
                CusAreaId = model.CusAreaId,
                CusAreaName = model.CusAreaName,
                CusBirthday = birthday,
                CusCityId = model.CusCityId,
                CusCityName = model.CusCityName,
                CusCurrentScore = model.CusCurrentScore,
                CusLevelId = model.CusLevelId,
                CusLocation = model.CusLocation,
                CusLogo = model.CusLogo,
                CusName = model.CusName,
                CusNo = GetCusNo(mctFlag, ud.MctNum),
                CusOldMemberNo = model.CusOldMemberNo,
                CusPhoneNo = model.CusPhoneNo,
                CusProvinceId = model.CusProvinceId,
                CusProvinceName = model.CusProvinceName,
                CusRegisterTime = model.CusRegisterTime,
                CusRemark = model.CusRemark,
                CusSex = model.CusSex,
                CusSource = model.CusSource,
                CusWechatNo = model.CusWechatNo,
                CusIsBindWX = false,
                Id = SecureHelper.GetNum(),
                LastModifyAccount = ud.Account,
                LastModifyTime = DateTime.Now,
                LastModifyUserId = ud.UserID,
                MctNum = ud.MctNum,
                Status = true,
                IsActivation = false,
                StoreId = ud.StoreId,
                IsDelete = false,
                CusFaceId = model.CusFaceId,
                CusInitialIntegral = (decimal)model.CusCurrentScore,
            };

            List<Cus_StaffCustomer> staffList = new List<Cus_StaffCustomer>();

            Cus_StaffCustomer staff = new Cus_StaffCustomer()
            {
                CustomerId = create.Id,
                DistributionId = "",
                DistributionTime = DateTime.Now,
                Id = SecureHelper.GetNum(),
                MctNum = ud.MctNum,
                StaffId = model.CusFollowPerson,
                StaffName = staffModel == null ? "" : staffModel.StaffName,
                StoreId = ud.StoreId,
            };
            staffList.Add(staff);

            List<Cus_CustomerTags> tags = new List<Cus_CustomerTags>();
            if (model.labels != null && model.labels.Count > 0)
            {
                var labelNames = new List<string>();
                var cusLabel = CustomerLabelService.GetLabelNameDropDown(ud);
                if (cusLabel != null && cusLabel.ReturnData != null && cusLabel.ReturnData.Count > 0)
                {
                    foreach (var item in model.labels)
                    {
                        var tag = cusLabel.ReturnData.Find(o => o.Name == item);
                        if (tag == null)
                        {
                            labelNames.Add(item);
                        }
                        else
                        {
                            Cus_CustomerTags tagModel = new Cus_CustomerTags()
                            {
                                CusId = create.Id,
                                Id = SecureHelper.GetNum(),
                                LabelId = tag.Id,
                                MctNum = ud.MctNum,
                                StoreId = ud.StoreId,
                            };
                            tags.Add(tagModel);
                        }
                    }
                }
                else
                {
                    labelNames = model.labels;
                }
                if (labelNames != null && labelNames.Count > 0)
                {
                    var resLabels = CustomerLabelService.CreateList(labelNames, ud);
                    if (resLabels.Success && resLabels.ReturnData != null)
                    {
                        foreach (var item in resLabels.ReturnData)
                        {
                            Cus_CustomerTags tagModel = new Cus_CustomerTags()
                            {
                                CusId = create.Id,
                                Id = SecureHelper.GetNum(),
                                LabelId = item.Id,
                                MctNum = ud.MctNum,
                                StoreId = ud.StoreId,
                            };
                            tags.Add(tagModel);
                        }
                    }
                    else
                    {
                        res.Success = false;
                        res.Message = "客户标签创建失败";
                        return res;
                    }
                }
            }
            List<Cus_IntegralRecord> integralList = new List<Cus_IntegralRecord>();
            if (model.CusCurrentScore > 0)
            {
                Cus_IntegralRecord integralRecord = new Cus_IntegralRecord()
                {
                    Id = SecureHelper.GetNum(),
                    StoreId = ud.StoreId,
                    IntegralRulesId = "0", //初始化积分
                    ERPOrderNo = null,
                    ChangeType = 1,
                    AffectedNumber = (decimal)create.CusCurrentScore,
                    AffectedMoney = 0,
                    ScoreBalance = (decimal)create.CusCurrentScore,
                    BusinessStoreId = ud.StoreId,
                    BusinessStaffId = ud.EmpId,
                    IntegralType = WJewel.DataContract.CRM.Common.CommonIntegralTypeModel.AddForOther,
                    MainSalerId = null,
                    MctNum = ud.MctNum,
                    Remark = "创建初始化积分",
                    CustomerId = create.Id,
                    CreatedDate = DateTime.Now
                };
                integralList.Add(integralRecord);
            }

            List<Cus_BaseCustomer> baseCustoms = new List<Cus_BaseCustomer>();
            var baseCustom = customData.GetBaseCustomByPhone(create.CusPhoneNo);
            if (baseCustom == null)
            {
                baseCustom = new Cus_BaseCustomer()
                {
                    ActivationTime = new DateTime(),
                    CusPhoneNo = create.CusPhoneNo,
                    Id = SecureHelper.GetNum(),
                    IsActivation = false,
                };
                baseCustoms.Add(baseCustom);
            }

            if (customData.Create(new List<Cus_Customer>() { create }, tags, integralList, staffList, baseCustoms))
            {
                if (model.CusFaceId > 0 && baseCustom != null)
                {
                    if (baseCustom.IsActivation == false)
                    {
                        //请求激活会员
                        ActivationCustomerModel actModel = new ActivationCustomerModel()
                        {
                            CustomerCode = create.Id,
                            FaceId = model.CusFaceId,
                        };
                        var activation = WisdomServiceTransfer.UpdateFaceType(actModel, ud.TokenStr);
                        if (activation)
                        {
                            //激活成功
                            customData.Activation(create.Id);
                            customData.BaseCustomActivation(create.CusPhoneNo);
                            new Task(() =>
                            {
                                WisdomServiceTransfer.UpdateFaceCustomer(new UpdateFaceCustomerModel()
                                {
                                    CusId = create.Id,
                                    CusLevelId = create.CusLevelId,
                                    CusLevelName = tempLevelName,
                                    CusName = create.CusName,
                                    CusNo = create.CusNo,
                                    CusPhoneNo = create.CusPhoneNo,
                                    FaceId = create.CusFaceId,
                                    MctNum = ud.MctNum,
                                    StaffId = staff.Id,
                                    StaffName = staff.StaffName,
                                    StoreId = storeIds.ToArray(),
                                }, ud.TokenStr);
                            }).Start();

                        }
                    }
                    else
                    {
                        customData.Activation(create.Id);
                        new Task(() =>
                        {
                            WisdomServiceTransfer.UpdateFaceCustomer(new UpdateFaceCustomerModel()
                            {
                                CusId = create.Id,
                                CusLevelId = create.CusLevelId,
                                CusLevelName = tempLevelName,
                                CusName = create.CusName,
                                CusNo = create.CusNo,
                                CusPhoneNo = create.CusPhoneNo,
                                FaceId = create.CusFaceId,
                                MctNum = ud.MctNum,
                                StaffId = staff.Id,
                                StaffName = staff.StaffName,
                                StoreId = storeIds.ToArray(),
                            }, ud.TokenStr);
                        }).Start();
                    }
                }

                SyncErp(create);

                res.Success = true;
                res.ReturnData = create.Id;
                res.Message = "创建成功";
            }
            else
            {
                res.Success = false;
                res.Message = "创建失败";
            }
            return res;
        }

        /// <summary>
        /// 编辑客户
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult<bool> Modify(ModifyCustomer model, UserData ud)
        {
            OperatResult<bool> res = new OperatResult<bool>();
            var custom = new Custom().GetById(model.Id);
            if (custom == null || custom.MctNum != ud.MctNum)
            {
                res.Success = false;
                res.Message = "未找到客户信息！";
                return res;
            }

            StaffDetailInfoModel staffModel = null;
            if (!string.IsNullOrWhiteSpace(model.CusFollowPerson))
            {
                staffModel = StoreAndStaffServiceTransfer.GetStaffDetailInfo(model.CusFollowPerson);
                if (staffModel == null)
                {
                    res.Success = false;
                    res.Message = "未找到跟进人的员工信息！";
                    return res;
                }
            }
            var tempLevelName = "";
            var score = 0M;
            var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
            var scoreLevel = GetCustomerLevel((decimal)custom.CusAccumulatedPoints, ud.MctNum);
            if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
            {
                var levelList = levelPageList.ReturnData.DataList;
                if (!string.IsNullOrWhiteSpace(model.CusLevelId))
                {
                    var level = levelList.Find(o => o.Id == model.CusLevelId);
                    if (level == null)
                    {
                        var tempLevel = scoreLevel != null && scoreLevel.ReturnData != null ? scoreLevel.ReturnData : levelList.Find(o => o.Grade == "普通客户");
                        model.CusLevelId = tempLevel != null ? tempLevel.Id : "";
                        tempLevelName = tempLevel != null ? tempLevel.Grade : "";
                        score = (decimal)(tempLevel != null ? tempLevel.AchieveConditions : 0);
                    }
                    else
                    {
                        tempLevelName = level != null ? level.Grade : "";
                        score = (decimal)(level != null ? level.AchieveConditions : 0);
                        model.CusLevelId = level.Id;
                    }
                }
                else
                {
                    var tempLevel = scoreLevel != null && scoreLevel.ReturnData != null ? scoreLevel.ReturnData : levelList.Find(o => o.Grade == "普通客户");
                    model.CusLevelId = tempLevel != null ? tempLevel.Id : "";
                    tempLevelName = tempLevel != null ? tempLevel.Grade : "";
                    score = (decimal)(tempLevel != null ? tempLevel.AchieveConditions : 0);
                }
            }
            else
            {
                res.Success = false;
                res.Message = "未找到客户等级信息！";
                return res;
            }
            if (scoreLevel != null && scoreLevel.ReturnData != null)
            {
                if (scoreLevel.ReturnData.Id != model.CusLevelId)
                {
                    var message = "";
                    if (score > scoreLevel.ReturnData.AchieveConditions)
                    {
                        message = "该客户初始积分匹配的会员等级为【" + scoreLevel.ReturnData.Grade + "】,请调整会员等级";
                    }
                    else if (score < scoreLevel.ReturnData.AchieveConditions)
                    {
                        message = "该客户初始积分匹配的会员等级为【" + scoreLevel.ReturnData.Grade + "】,请调整会员等级。";
                    }
                    res.Success = false;
                    res.Message = message;
                    return res;
                }
            }
            List<string> storeIds = new List<string>();
            var storeList = StoreAndStaffServiceTransfer.GetStoreList(ud.TokenStr); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
            if (storeList != null && storeList.Count > 0)
            {
                var store = storeList.Find(o => o.Id == ud.StoreId);
                if (store == null)
                {
                    storeIds.Add(ud.StoreId); //改组没有查询到自身信息，直接返回自身
                }
                else
                {
                    if (store.IsShareMemberAIntegral)
                    {
                        foreach (var item in storeList)
                        {
                            storeIds.Add(item.Id);
                        }
                    }
                    else
                    {
                        storeIds.Add(ud.StoreId); //门店组没有共享会员信息
                    }
                }
            }
            else
            {
                storeIds.Add(ud.StoreId); //没有查询到门店组信息，直接返回自身
            }
            var resCustom = new Custom().GetByPhone(ud.MctNum, model.CusPhoneNo, storeIds.ToArray(), model.Id);
            if (resCustom != null)
            {
                if (resCustom.StoreId == ud.StoreId)
                {
                    res.Success = false;
                    res.Message = "客户已存在！";
                    return res;
                }
                else
                {
                    res.Success = false;
                    res.Message = "客户已存在共享！";
                    return res;
                }
            }
            if (model.CusFaceId > 0)
            {
                var cus = new Custom().GetByFaceId(model.CusFaceId, ud.MctNum, storeIds.ToArray());
                if (cus != null && cus.Id != model.Id)
                {
                    res.Success = false;
                    res.Message = "客户号为：【" + cus.CusNo + "】的客户已绑定了此人脸信息！";
                    return res;
                }
            }
            Cus_Customer modify = new Cus_Customer()
            {
                CreateAccount = custom.CreateAccount,
                CreateTime = custom.CreateTime,
                CreateUserId = custom.CreateUserId,
                CusAccumulatedPoints = custom.CusAccumulatedPoints,
                CusAddress = model.CusAddress,
                CusAreaId = model.CusAreaId,
                CusAreaName = model.CusAreaName,
                CusBirthday = model.CusBirthday,
                CusCityId = model.CusCityId,
                CusCityName = model.CusCityName,
                CusCurrentScore = custom.CusCurrentScore,
                CusInitialIntegral = custom.CusInitialIntegral,
                CusLevelId = model.CusLevelId,
                CusLocation = model.CusLocation,
                CusLogo = model.CusLogo,
                CusName = model.CusName,
                CusNo = custom.CusNo,
                CusOldMemberNo = model.CusOldMemberNo,
                CusPhoneNo = model.CusPhoneNo,
                CusProvinceId = model.CusProvinceId,
                CusProvinceName = model.CusProvinceName,
                CusRegisterTime = custom.CusRegisterTime,
                CusRemark = model.CusRemark,
                CusSex = model.CusSex,
                CusSource = string.IsNullOrWhiteSpace(custom.CusSource) ? model.CusSource : custom.CusSource,
                CusWechatNo = model.CusWechatNo,
                CusIsBindWX = custom.CusIsBindWX,
                Id = custom.Id,
                LastModifyAccount = ud.Account,
                LastModifyTime = DateTime.Now,
                LastModifyUserId = ud.UserID,
                MctNum = ud.MctNum,
                Status = custom.Status,
                IsActivation = custom.IsActivation,
                StoreId = custom.StoreId,
                CusFaceId = model.CusFaceId,
                IsDelete = custom.IsDelete,
            };

            List<Cus_CustomerTags> tags = new List<Cus_CustomerTags>();
            if (model.labels != null && model.labels.Count > 0)
            {
                var labelNames = new List<string>();
                var cusLabel = CustomerLabelService.GetLabelNameDropDown(ud);
                if (cusLabel != null && cusLabel.ReturnData != null && cusLabel.ReturnData.Count > 0)
                {
                    foreach (var item in model.labels)
                    {
                        var tag = cusLabel.ReturnData.Find(o => o.Name == item);
                        if (tag == null)
                        {
                            labelNames.Add(item);
                        }
                        else
                        {
                            Cus_CustomerTags tagModel = new Cus_CustomerTags()
                            {
                                CusId = modify.Id,
                                Id = SecureHelper.GetNum(),
                                LabelId = tag.Id,
                                MctNum = ud.MctNum,
                                StoreId = ud.StoreId,
                            };
                            tags.Add(tagModel);
                        }
                    }
                }
                else
                {
                    labelNames = model.labels;
                }
                if (labelNames != null && labelNames.Count > 0)
                {
                    var resLabels = CustomerLabelService.CreateList(labelNames, ud);
                    if (resLabels.Success && resLabels.ReturnData != null)
                    {
                        foreach (var item in resLabels.ReturnData)
                        {
                            Cus_CustomerTags tagModel = new Cus_CustomerTags()
                            {
                                CusId = modify.Id,
                                Id = SecureHelper.GetNum(),
                                LabelId = item.Id,
                                MctNum = ud.MctNum,
                                StoreId = ud.StoreId,
                            };
                            tags.Add(tagModel);
                        }
                    }
                    else
                    {
                        res.Success = false;
                        res.Message = "客户标签创建失败";
                        return res;
                    }
                }
            }
            List<Cus_StaffCustomer> staffList = new List<Cus_StaffCustomer>();
            Cus_StaffCustomer staff = new Cus_StaffCustomer()
            {
                CustomerId = modify.Id,
                DistributionId = "",
                DistributionTime = DateTime.Now,
                Id = SecureHelper.GetNum(),
                MctNum = ud.MctNum,
                StaffId = staffModel == null ? "" : staffModel.Id,
                StaffName = staffModel == null ? "" : staffModel.StaffName,
                StoreId = ud.StoreId,
            };
            staffList.Add(staff);

            List<Cus_BaseCustomer> baseCustoms = null;
            var baseCustom = new Custom().GetBaseCustomByPhone(model.CusPhoneNo);
            if (baseCustom == null)
            {
                baseCustom = new Cus_BaseCustomer()
                {
                    ActivationTime = new DateTime(),
                    CusPhoneNo = model.CusPhoneNo,
                    Id = SecureHelper.GetNum(),
                    IsActivation = false,
                };
                baseCustoms = new List<Cus_BaseCustomer>();
                baseCustoms.Add(baseCustom);
            }
            if (new Custom().Update(new List<Cus_Customer>() { modify }, tags, staffList, baseCustoms))
            {
                if (model.CusFaceId > 0 && model.CusFaceId != custom.CusFaceId)
                {
                    if (baseCustom.IsActivation == false)
                    {
                        //请求激活会员
                        ActivationCustomerModel actModel = new ActivationCustomerModel()
                        {
                            CustomerCode = model.Id,
                            FaceId = model.CusFaceId,
                        };
                        var activation = WisdomServiceTransfer.UpdateFaceType(actModel, ud.TokenStr);
                        if (activation)
                        {
                            //激活成功
                            new Custom().Activation(modify.Id);
                            new Custom().BaseCustomActivation(modify.CusPhoneNo);
                        }
                    }
                    else
                    {
                        new Custom().Activation(modify.Id);
                    }
                }
                new Task(() =>
                {
                    WisdomServiceTransfer.UpdateFaceCustomer(new UpdateFaceCustomerModel()
                    {
                        CusId = modify.Id,
                        CusLevelId = modify.CusLevelId,
                        CusLevelName = tempLevelName,
                        CusName = modify.CusName,
                        CusNo = modify.CusNo,
                        CusPhoneNo = modify.CusPhoneNo,
                        FaceId = modify.CusFaceId,
                        MctNum = ud.MctNum,
                        StaffId = staff.Id,
                        StaffName = staff.StaffName,
                        StoreId = storeIds.ToArray(),
                    }, ud.TokenStr);
                }).Start();

                SyncErp(modify);

                res.Success = true;
                res.ReturnData = true;
                res.Message = "编辑成功";
            }
            else
            {
                res.Success = false;
                res.Message = "编辑失败";
            }
            return res;
        }

        /// <summary>
        /// 同步/更新会员数据
        /// </summary>
        /// <param name="create"></param>
        public static void SyncErp(Cus_Customer create)
        {
            new Task(() =>
            {
                if (create == null)
                    return;
                if (string.IsNullOrEmpty(create.CusNo) || string.IsNullOrEmpty(create.CusPhoneNo))
                    return;

                var shop = new ERPStore().GetInfoByStoreId(create.StoreId);//匹配门店同步数据
                if (shop == null)
                    return;
                //LogHelper.WriteSysFileLog("CustomerService", "SyncErp", "", "System", "同步会员开始", $"会员参数：{JSONHelper.Encode(create)}", "");
                WebCrawling web = new WebCrawling(create.MctNum);
                if (!web.IsSync)
                    return;


                var sync = new SyncDataMap();
                var syncdata = sync.GetInfoByLocalId(create.Id);//查询当前会员同步记录
                var OriginId = "";
                if (syncdata != null)//如果有同步记录
                {
                    var origin = web.loadCustomer(keyword: syncdata.Mobile);//获取ERP数据列表
                    if (origin != null && origin.Count > 0)
                    {
                        var erpcustomer = origin.Where(t => t.mobile == syncdata.Mobile);
                        if (erpcustomer != null)//匹配到erp数据，更新处理
                        {
                            var temp = erpcustomer.FirstOrDefault();
                            var customerModel = web.ErpCustomerModelMap(temp);
                            if (syncdata.Mobile != create.CusPhoneNo || (!string.IsNullOrEmpty(create.CusNo) && customerModel.vipCardNo != create.CusNo))
                            {
                                customerModel.cusName = create.CusName;
                                customerModel.mobile = create.CusPhoneNo;
                                if (!string.IsNullOrEmpty(create.CusNo) && customerModel.vipCardNo != create.CusNo)
                                    customerModel.vipCardNo = create.CusNo;

                                var s = web.updateCustomerBaseInfo(customerModel);

                                if (s.status.ToLower() == "success" && syncdata.Mobile != create.CusPhoneNo)
                                {
                                    new SyncDataMap().ModifyMobile(syncdata.Id, create.CusPhoneNo);
                                }
                                LogHelper.WriteSysFileLog("CustomerService", "SyncErp", "", "System", "同步会员结束", $"会员信息更新数据：{JSONHelper.Encode(customerModel)}", "");
                            }
                        }
                    }
                }
                else
                {
                    var v = web.vipCardNoAndMobileCheck(create.CusPhoneNo, Convert.ToString(shop.ERPId), create.CusNo);
                    if (v.status == "error")
                    {
                        var erpcustomers = web.loadCustomer(keyword: create.CusPhoneNo);
                        if (erpcustomers != null && erpcustomers.Count > 0)
                        {
                            var erpcustomer = erpcustomers.Where(t => t.mobile == create.CusPhoneNo);
                            if (erpcustomer != null)//匹配到erp数据，更新处理
                            {
                                var temp = erpcustomer.FirstOrDefault();
                                OriginId = temp.id.ToString();
                                var customerModel = web.ErpCustomerModelMap(temp);
                                if (syncdata.Mobile != create.CusPhoneNo || (!string.IsNullOrEmpty(create.CusNo) && customerModel.vipCardNo != create.CusNo))
                                {
                                    customerModel.cusName = create.CusName;
                                    customerModel.mobile = create.CusPhoneNo;
                                    if (!string.IsNullOrEmpty(create.CusNo) && customerModel.vipCardNo != create.CusNo)
                                        customerModel.vipCardNo = create.CusNo;
                                    web.updateCustomerBaseInfo(customerModel);

                                    LogHelper.WriteSysFileLog("CustomerService", "SyncErp", "", "System", "同步会员结束", $"会员信息更新数据：{JSONHelper.Encode(customerModel)}", "");
                                }
                            }
                        }
                    }
                    else
                    {
                        var zberpcustomer = new ZbErpCustomerModel()
                        {
                            integralDegree = 35,
                            cusName = create.CusName,
                            mobile = create.CusPhoneNo,
                            vipCardNo = create.CusNo,
                            vipCardLevelId = 171,
                            vipCardLevel = "VIP",
                            sex = create.CusSex ? 1 : 2,
                            sendDate = DateTime.Now.ToString("yyyy-MM-dd"),
                            shopId = Convert.ToInt32(shop.ERPId),
                            shopName = shop.AreaName,
                            birthdayType = 1,
                        };
                        var s = web.saveCustomerBaseInfo(zberpcustomer);
                        if (s.status.ToLower() == "success")
                        {
                            var erpcustomers = web.loadCustomer(keyword: create.CusPhoneNo);

                            if (erpcustomers != null && erpcustomers.Count > 0)
                            {
                                var erpcustomer = erpcustomers.Where(t => t.mobile == create.CusPhoneNo);

                                if (erpcustomer != null)
                                {
                                    var temp = erpcustomer.FirstOrDefault();
                                    OriginId = temp.id.ToString();
                                }
                            }
                            LogHelper.WriteSysFileLog("CustomerService", "SyncErp", "", "System", "同步会员成功", $"同步数据：{JSONHelper.Encode(zberpcustomer)}，同步结果唯一编号{OriginId}", "");
                        }
                    }
                    var syncinsterdata = new Sync_DataMap()
                    {
                        Id = SecureHelper.GetNum(),
                        MctNum = create.MctNum,
                        StoreId = create.StoreId,
                        LocalId = create.Id,
                        OriginId = OriginId,
                        Mobile = create.CusPhoneNo,
                        CardNo = create.CusNo,
                        SystemType = "zberp",
                        SyncType = "customer",
                        SyncTime = DateTime.Now,
                        IsEnble = true
                    };
                    new SyncDataMap().Create(new List<Sync_DataMap>() { syncinsterdata });
                }
            }).Start();
        }
        /// <summary>
        /// 更新客户标签
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult ModifyCustomerTags(ModifyCustomerTags model, UserData ud)
        {
            OperatResult res = new OperatResult();
            if (model.customerIds != null && model.customerIds.Count > 0)
            {
                List<Cus_CustomerTags> cusTags = new List<Cus_CustomerTags>();
                foreach (var item in model.customerIds)
                {
                    List<string> storeIds = new List<string>() { ud.StoreId };
                    var chief = StoreAndStaffServiceTransfer.GetChiefByMct(ud.MctNum);
                    if (chief != null && chief.Id != ud.StoreId)
                    {
                        storeIds.Add(chief.Id);
                    }
                    var tags = new Data.CustomerTags().GetList(item, ud.MctNum, storeIds.ToArray());
                    if (tags != null && model.labelIds != null && model.labelIds.Count > 0)
                    {
                        var labelIds = tags.Select(o => o.LabelId);
                        foreach (var tag in model.labelIds)
                        {
                            if (!labelIds.Contains(tag))
                            {
                                Cus_CustomerTags tagModel = new Cus_CustomerTags()
                                {
                                    CusId = item,
                                    Id = SecureHelper.GetNum(),
                                    LabelId = tag,
                                    MctNum = ud.MctNum,
                                    StoreId = ud.StoreId,
                                };
                                cusTags.Add(tagModel);
                            }
                        }
                    }
                    else
                    {
                        if (model.labelIds != null && model.labelIds.Count > 0)
                        {
                            foreach (var tag in model.labelIds)
                            {
                                Cus_CustomerTags tagModel = new Cus_CustomerTags()
                                {
                                    CusId = item,
                                    Id = SecureHelper.GetNum(),
                                    LabelId = tag,
                                    MctNum = ud.MctNum,
                                    StoreId = ud.StoreId,
                                };
                                cusTags.Add(tagModel);
                            }
                        }
                    }
                }
                if (new Data.CustomerTags().Create(cusTags))
                {
                    res.Message = "创建标签成功";
                    res.Success = true;
                }
                else
                {
                    res.Message = "创建标签成功";
                    res.Success = false;
                }
            }
            return res;
        }

        /// <summary>
        /// 更新客户标签(微信员工通道)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult ModifyCustomerTagsForCrm(ModifyCustomerTags model, UserData ud)
        {
            OperatResult res = new OperatResult();
            if (model.customerIds != null && model.customerIds.Count > 0)
            {
                List<string> storeIds = new List<string>() { ud.StoreId };
                var chief = StoreAndStaffServiceTransfer.GetChiefByMct(ud.MctNum);
                if (chief != null && chief.Id != ud.StoreId)
                {
                    storeIds.Add(chief.Id);
                }
                List<Cus_CustomerTags> cusTags = new List<Cus_CustomerTags>();
                string cusId = "";
                foreach (var item in model.customerIds)
                {
                    if (model.labelIds != null && model.labelIds.Count > 0)
                    {
                        foreach (var tag in model.labelIds)
                        {
                            Cus_CustomerTags tagModel = new Cus_CustomerTags()
                            {
                                CusId = item,
                                Id = SecureHelper.GetNum(),
                                LabelId = tag,
                                MctNum = ud.MctNum,
                                StoreId = ud.StoreId,
                            };
                            cusTags.Add(tagModel);
                        }
                    }
                    cusId = item;
                }
                if (new Data.CustomerTags().ModifyCusTag(cusTags, ud.StoreId, cusId))
                {
                    res.Message = "创建标签成功";
                    res.Success = true;
                }
                else
                {
                    res.Message = "创建标签成功";
                    res.Success = false;
                }
            }
            return res;
        }

        /// <summary>
        /// 获取列表(本门店)
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="vague"></param>
        /// <param name="cusName"></param>
        /// <param name="cusNo"></param>
        /// <param name="phoneNo"></param>
        /// <param name="cusSource"></param>
        /// <param name="cusFollowPerson"></param>
        /// <param name="status"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static OperatResult<PageList<CustomerListModel>> GetList(UserData ud, string levelId = "", string vague = "", string cusName = "", string cusNo = "", string phoneNo = "",
            string cusSource = "", string cusFollowPerson = "", int status = -1, int isActivation = -1, int pageIndex = 1, int pageSize = 15)
        {
            OperatResult<PageList<CustomerListModel>> res = new OperatResult<PageList<CustomerListModel>>();
            PageList<CustomerListModel> pageList = new PageList<CustomerListModel>();
            var totalCount = 0;
            var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
            bool IsOrdinaryCustom = false;
            if (levelPageList != null && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null && levelPageList.ReturnData.DataList.Count > 0)
            {
                CustomerLevelModel levelModel = levelPageList.ReturnData.DataList.Find(o => o.Id == levelId);
                if (levelModel != null && levelModel.Grade == "普通客户")
                {
                    IsOrdinaryCustom = true;
                }
            }
            var ret = new Custom().GetList(ud.MctNum, ref totalCount, new string[] { ud.StoreId }, ud.StoreId, levelId, IsOrdinaryCustom, vague, cusName, cusNo, phoneNo, cusSource, cusFollowPerson, status, isActivation, pageIndex, pageSize);
            if (ret != null && ret.Count > 0)
            {
                List<CustomerLevelModel> levelList = null;
                if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
                {
                    levelList = levelPageList.ReturnData.DataList;
                }
                var staffList = new StaffCustomer().GetList(ud.StoreId);
                List<CustomerListModel> list = new List<CustomerListModel>();
                foreach (var item in ret)
                {
                    Cus_StaffCustomer staffModel = null;
                    if (staffList != null && staffList.Count > 0)
                    {
                        staffModel = staffList.Find(o => o.CustomerId == item.Id);
                    }
                    CustomerLevelModel tempLevel = null;
                    if (string.IsNullOrWhiteSpace(item.CusLevelId))
                    {
                        tempLevel = levelList.Find(o => o.Grade == "普通客户");
                    }
                    else
                    {
                        tempLevel = levelList.Find(o => o.Id == item.CusLevelId);
                        if (tempLevel == null)
                            tempLevel = levelList.Find(o => o.Grade == "普通客户");
                    }
                    if (tempLevel == null)
                        tempLevel = new CustomerLevelModel();
                    CustomerListModel model = new CustomerListModel()
                    {
                        CusAccumulatedPoints = item.CusAccumulatedPoints,
                        CusBirthday = item.CusBirthday,
                        CusCurrentScore = item.CusCurrentScore,
                        CusFollowPersonName = staffModel != null ? staffModel.StaffName : "",
                        CusLevelId = tempLevel != null ? tempLevel.Id : "",
                        CusLogo = item.CusLogo,
                        CusName = item.CusName,
                        CusNo = item.CusNo,
                        CusPhoneNo = item.CusPhoneNo,
                        CusRemark = item.CusRemark,
                        CusSex = item.CusSex ? "男" : "女",
                        CusWechatNo = item.CusWechatNo,
                        IsActivation = item.IsActivation,
                        Id = item.Id,
                        Status = item.Status,
                        LevelGrade = tempLevel != null ? tempLevel.Grade : "",
                        BirthdayConsumption = tempLevel != null ? tempLevel.BirthdayConsumption : 0,
                        OrdinaryConsumption = tempLevel != null ? tempLevel.OrdinaryConsumption : 0,
                        OtherEquity = tempLevel != null ? tempLevel.OtherEquity : "",
                        ProductOffer = tempLevel != null ? tempLevel.ProductOffer : "",
                        CusInitialIntegral = item.CusInitialIntegral,
                        IsShare = item.StoreId != ud.StoreId ? true : false,
                    };
                    list.Add(model);
                }
                pageList.Page = pageIndex;
                pageList.PageSize = pageSize;
                pageList.TotalCount = totalCount;
                pageList.DataList = list;

                res.Success = true;
                res.Message = "获取成功";
                res.ReturnData = pageList;
            }
            else
            {
                res.Success = true;
                res.Message = "暂无数据";
                res.ReturnData = null;
            }
            return res;
        }

        #region 统计报表使用
        /// <summary>
        /// 获取按等级统计客户列表
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="storeId"></param>
        /// <param name="beginRegistedDate"></param>
        /// <param name="endRegistedDate"></param>
        /// <returns></returns>
        public static OperatResult<List<GetListByLevelModel>> GetListByLevel(UserData ud, string storeId, string beginRegistedDate, string endRegistedDate)
        {
            OperatResult<List<GetListByLevelModel>> ret = new OperatResult<List<GetListByLevelModel>>();
            List<GetListByLevelModel> retList = new List<GetListByLevelModel>();
            //所有客户等级
            var levelPageListRet = CustomerLevelService.GetList(ud, string.Empty, 1, 10000);
            //所有客户
            List<Cus_Customer> customerList = new Custom().GetList(ud.MctNum, new string[] { storeId }, beginRegistedDate, endRegistedDate);
            if (levelPageListRet != null && levelPageListRet.ReturnData != null && levelPageListRet.ReturnData.DataList != null && levelPageListRet.ReturnData.DataList.Count > 0)
            {
                var levelList = levelPageListRet.ReturnData.DataList;
                foreach (var level in levelList)
                {
                    GetListByLevelModel retModel = new GetListByLevelModel();
                    retModel.CustomerLevelId = level.Id;
                    retModel.CustomerLevelName = level.Grade;
                    retModel.CustomerCount = customerList.FindAll(s => s.CusLevelId == level.Id).Count;
                }
                ret.Success = true;
                ret.Message = "获取成功";
                ret.ReturnData = retList;
            }
            else
            {
                ret.Success = true;
                ret.Message = "暂无数据";
                ret.ReturnData = null;
            }
            return ret;
        }

        /// <summary>
        /// 获取商户下客户
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="storeId"></param>
        /// <param name="beginRegistedDate"></param>
        /// <param name="endRegistedDate"></param>
        /// <returns></returns>
        public static OperatResult<List<CustomerModel>> GetAllList(UserData ud, string storeId, string beginRegistedDate, string endRegistedDate)
        {
            OperatResult<List<CustomerModel>> ret = new OperatResult<List<CustomerModel>>();
            List<CustomerModel> retList = new List<CustomerModel>();

            var storeList = storeId.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            //所有客户
            List<Cus_Customer> customerList = new Custom().GetList(ud.MctNum, storeList, beginRegistedDate, endRegistedDate);
            if (customerList != null && customerList.Count > 0)
            {
                foreach (var customer in customerList)
                {
                    CustomerModel retModel = ObjectHelper.NewMapper<CustomerModel, Cus_Customer>(customer);
                    retList.Add(retModel);
                }
                ret.Success = true;
                ret.Message = "获取成功";
                ret.ReturnData = retList;
            }
            else
            {
                ret.Success = true;
                ret.Message = "暂无数据";
                ret.ReturnData = null;
            }
            return ret;
        }
        #endregion


        /// <summary>
        /// 获取列表(CRM员工通道用)
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="levelIds"></param>
        /// <param name="isBindWx"></param>
        /// <param name="keyWord"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static OperatResult<PageList<CustomerListModel>> GetShareListByCrm(UserData ud, List<string> levelIds, int isBindWx, int isActivation, string keyWord = "", int pageIndex = 1, int pageSize = 15)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            OperatResult<PageList<CustomerListModel>> res = new OperatResult<PageList<CustomerListModel>>();
            PageList<CustomerListModel> pageList = new PageList<CustomerListModel>();
            var totalCount = 0;
            var chief = StoreAndStaffServiceTransfer.GetChiefByMct(ud.MctNum);
            var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
            bool IsOrdinaryCustom = false;
            if (levelPageList != null && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null && levelPageList.ReturnData.DataList.Count > 0)
            {
                if (levelIds != null && levelIds.Count > 0)
                {
                    foreach (var levelId in levelIds)
                    {
                        CustomerLevelModel levelModel = levelPageList.ReturnData.DataList.Find(o => o.Id == levelId);
                        if (levelModel != null && levelModel.Grade == "普通客户")
                        {
                            IsOrdinaryCustom = true;
                        }
                    }
                }
            }
            sw.Stop();
            new Task(() =>
            {
                LogHelper.WriteSysFileLog("Customer", "GetShareList", "Info", "客户", "LevelService", sw.Elapsed.ToString(), "127.0.0.1");
            }).Start();
            List<string> storeIds = new List<string>() { ud.StoreId };
            var storeList = StoreAndStaffServiceTransfer.GetStoreList(ud.TokenStr); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
            if (storeList != null && storeList.Count > 0)
            {
                var store = storeList.Find(o => o.Id == ud.StoreId);
                if (store != null)
                {
                    if (store.IsShareMemberAIntegral)
                    {
                        foreach (var item in storeList)
                        {
                            storeIds.Add(item.Id);
                        }
                    }
                }
            }
            var ret = new Custom().GetListByCrm(ud.MctNum, ref totalCount, storeIds.ToArray(), levelIds == null ? null : levelIds.ToArray(), ud.StoreId, IsOrdinaryCustom, isBindWx, isActivation, keyWord, pageIndex, pageSize);
            if (ret != null && ret.Count > 0)
            {
                List<CustomerLevelModel> levelList = null;
                if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
                {
                    levelList = levelPageList.ReturnData.DataList;
                }
                var staffList = new StaffCustomer().GetList(ud.StoreId);
                List<CustomerListModel> list = new List<CustomerListModel>();
                foreach (var item in ret)
                {
                    CustomerLevelModel tempLevel = null;
                    if (string.IsNullOrWhiteSpace(item.CusLevelId))
                    {
                        tempLevel = levelList.Find(o => o.Grade == "普通客户");
                    }
                    else
                    {
                        tempLevel = levelList.Find(o => o.Id == item.CusLevelId);
                        if (tempLevel == null)
                            tempLevel = levelList.Find(o => o.Grade == "普通客户");
                    }
                    if (tempLevel == null)
                        tempLevel = new CustomerLevelModel();
                    Cus_StaffCustomer staffModel = null;
                    if (staffList != null && staffList.Count > 0)
                    {
                        staffModel = staffList.Find(o => o.CustomerId == item.Id);
                    }
                    CustomerListModel model = new CustomerListModel()
                    {
                        CusAccumulatedPoints = item.CusAccumulatedPoints,
                        CusBirthday = item.CusBirthday,
                        CusCurrentScore = item.CusCurrentScore,
                        CusFollowPersonName = staffModel != null ? staffModel.StaffName : "",
                        CusLevelId = tempLevel != null ? tempLevel.Id : "",
                        CusLogo = item.IsActivation ? item.CusLogo : item.CusWxLogo,
                        CusName = item.CusName,
                        CusNo = item.CusNo,
                        CusPhoneNo = item.CusPhoneNo,
                        CusRemark = item.CusRemark,
                        CusSex = item.CusSex ? "男" : "女",
                        CusWechatNo = item.CusWechatNo,
                        IsActivation = item.IsActivation,
                        Id = item.Id,
                        Status = item.Status,
                        CusIsBindWX = item.CusIsBindWX,
                        LevelGrade = tempLevel != null ? tempLevel.Grade : "",
                        BirthdayConsumption = tempLevel != null ? tempLevel.BirthdayConsumption : 0,
                        OrdinaryConsumption = tempLevel != null ? tempLevel.OrdinaryConsumption : 0,
                        OtherEquity = tempLevel != null ? tempLevel.OtherEquity : "",
                        ProductOffer = tempLevel != null ? tempLevel.ProductOffer : "",
                        CusInitialIntegral = item.CusInitialIntegral,
                        IsShare = item.StoreId != ud.StoreId ? true : false,
                    };
                    list.Add(model);
                }
                pageList.Page = pageIndex;
                pageList.PageSize = pageSize;
                pageList.TotalCount = totalCount;
                pageList.DataList = list;

                res.Success = true;
                res.Message = "获取成功";
                res.ReturnData = pageList;
            }
            else
            {
                res.Success = true;
                res.Message = "暂无数据";
                res.ReturnData = null;
            }
            return res;
        }

        /// <summary>
        /// 获取下拉列表（本门店）
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="cusName"></param>
        /// <param name="cusPhone"></param>
        /// <returns></returns>
        public static OperatResult<List<CustomerDropDown>> GetDropDown(UserData ud, string cusName = "", string cusPhone = "", int status = -1)
        {
            OperatResult<List<CustomerDropDown>> res = new OperatResult<List<CustomerDropDown>>();
            List<CustomerDropDown> resList = new List<CustomerDropDown>();
            var totalCount = 0;
            var ret = new Custom().GetList(ud.MctNum, ref totalCount, new string[] { ud.StoreId }, ud.StoreId, "", false, "", cusName, "", cusPhone, "", "", status, -1, 1, 1000000);
            if (ret != null && ret.Count > 0)
            {
                foreach (var item in ret)
                {
                    CustomerDropDown model = new CustomerDropDown()
                    {
                        Id = item.Id,
                        Name = item.CusName,
                        Phone = item.CusPhoneNo,
                    };
                    resList.Add(model);
                }
                res.Success = true;
                res.ReturnData = resList;
                res.Message = "没有数据";
            }
            else
            {
                res.Success = true;
                res.ReturnData = null;
                res.Message = "没有数据";
            }
            return res;
        }

        /// <summary>
        /// 获取下拉列表（总部获取所有门店客户，门店获取自己门店客户）
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="cusName"></param>
        /// <param name="cusPhone"></param>
        /// <returns></returns>
        public static OperatResult<List<CustomerDropDownModel>> GetCustomerList(UserData ud)
        {
            OperatResult<List<CustomerDropDownModel>> res = new OperatResult<List<CustomerDropDownModel>>() { Success = false };
            var totalCount = 0;
            var storeInfo = StoreAndStaffServiceTransfer.GetStoreById(ud.StoreId, ud.TokenStr);
            if (storeInfo == null)
            {
                res.Message = "非总部和门店用户无法操作";
                return res;
            }
            List<string> storeIds = new List<string>();
            storeIds.Add(ud.StoreId);
            if (storeInfo.IsChief)
            {
                var storeList = StoreAndStaffServiceTransfer.GetStoreList(ud.TokenStr); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
                if (storeList != null && storeList.Count > 0)
                {
                    storeIds = storeList.Select(s => s.Id).ToList();
                }
            }
            var ret = new Custom().GetList(ud.MctNum, ref totalCount, storeIds.ToArray(), "", "", false, "", "", "", "", "", "", 1, -1, 1, 1000000);
            if (ret != null && ret.Count > 0)
            {
                res.ReturnData = new List<CustomerDropDownModel>();
                foreach (var item in ret)
                {
                    CustomerDropDownModel model = new CustomerDropDownModel()
                    {
                        Id = item.Id,
                        Name = item.CusName,
                        Phone = item.CusPhoneNo,
                        CusNo = item.CusNo
                    };
                    if (!res.ReturnData.Any(s => s.Phone == item.CusPhoneNo))
                        res.ReturnData.Add(model);
                }
                res.Success = true;
            }
            else
            {
                res.Success = true;
                res.Message = "没有数据";
            }
            return res;
        }



        /// <summary>
        /// 员工离职后更新跟进人信息
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public static OperatResult ModifyFollowStatus(UserData ud, string staffId)
        {
            OperatResult res = new OperatResult();
            if (new Custom().ModifyFollowStatus(staffId, ud.StoreId))
            {
                WisdomServiceTransfer.CleanFollowStaff(ud.TokenStr, staffId);
                res.Success = true;
                res.Message = "更新成功";
            }
            else
            {
                res.Success = false;
                res.Message = "更新失败";
            }
            return res;
        }

        /// <summary>
        /// 员工名称变更后后更新跟进人信息
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public static OperatResult ModifyFollowName(UserData ud, string staffId, string staffName)
        {
            OperatResult res = new OperatResult();
            if (new Custom().ModifyFollowName(staffId, ud.StoreId, staffName))
            {
                res.Success = true;
                res.Message = "更新成功";
            }
            else
            {
                res.Success = false;
                res.Message = "更新失败";
            }
            return res;
        }

        /// <summary>
        /// 根据编号获取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult<CustomerModel> GetById(string id, UserData ud)
        {
            OperatResult<CustomerModel> ret = new OperatResult<CustomerModel>();
            var res = new Custom().GetById(id);
            if (res == null || res.MctNum != ud.MctNum)
            {
                ret.Success = true;
                ret.Message = "未找到客户信息";
                ret.ReturnData = null;
                return ret;
            }

            var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
            List<CustomerLevelModel> levelList = null;
            if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
            {
                levelList = levelPageList.ReturnData.DataList;
            }
            if (levelList == null || levelList.Count == 0)
            {
                ret.Success = true;
                ret.Message = "数据错误，没有配置客户等级信息";
                ret.ReturnData = null;
                return ret;
            }
            CustomerLevelModel levelModel = null;
            if (string.IsNullOrWhiteSpace(res.CusLevelId))
            {
                levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            else
            {
                levelModel = levelList.Find(o => o.Id == res.CusLevelId);
                if (levelModel == null)
                    levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            if (levelModel == null)
                levelModel = new CustomerLevelModel();
            List<string> storeIds = new List<string>() { ud.StoreId };
            var chief = StoreAndStaffServiceTransfer.GetChiefByMct(ud.MctNum);
            if (chief != null && chief.Id != ud.StoreId)
            {
                storeIds.Add(chief.Id);
            }
            var tags = new Data.CustomerTags().GetList(res.Id, ud.MctNum, storeIds.ToArray());
            List<WJewel.DataContract.CRM.Customer.CustomerTags> cusTags = new List<WJewel.DataContract.CRM.Customer.CustomerTags>();
            if (tags != null && tags.Count > 0)
            {
                var labels = CustomerLabelService.GetLabelNameDropDown(ud);
                if (labels != null && labels.ReturnData != null)
                {
                    foreach (var item in tags)
                    {
                        var label = labels.ReturnData.Find(o => o.Id == item.LabelId);
                        cusTags.Add(new WJewel.DataContract.CRM.Customer.CustomerTags() { Id = item.Id, Name = label == null ? "" : label.Name, LabelId = item.LabelId });
                    }
                }
            }
            var staffModel = new StaffCustomer().GetInfo(ud.StoreId, res.Id);
            CustomerModel retModel = new CustomerModel()
            {
                BirthdayConsumption = levelModel.BirthdayConsumption,
                CusAccumulatedPoints = res.CusAccumulatedPoints,
                CusAddress = res.CusAddress,
                CusAreaId = res.CusAreaId,
                CusAreaName = res.CusAreaName,
                CusBirthday = res.CusBirthday,
                CusCityId = res.CusCityId,
                CusCityName = res.CusCityName,
                CusCurrentScore = res.CusCurrentScore,
                CusFollowPerson = staffModel != null ? staffModel.StaffId : "",
                CusFollowPersonName = staffModel != null ? staffModel.StaffName : "",
                CusLevelId = levelModel.Id,
                CusLocation = res.CusLocation,
                CusLogo = res.CusLogo,
                CusName = res.CusName,
                CusNo = res.CusNo,
                CusOldMemberNo = res.CusOldMemberNo,
                CusPhoneNo = res.CusPhoneNo,
                CusProvinceId = res.CusProvinceId,
                CusProvinceName = res.CusProvinceName,
                CusRemark = res.CusRemark,
                CusSex = res.CusSex ? "男" : "女",
                CusWechatNo = res.CusWechatNo,
                CusIsBindWX = res.CusIsBindWX, //客户是否已绑定微信
                Id = res.Id,
                IsActivation = res.IsActivation,
                LevelGrade = levelModel.Grade,
                OrdinaryConsumption = levelModel.OrdinaryConsumption,
                OtherEquity = levelModel.OtherEquity,
                ProductOffer = levelModel.ProductOffer,
                Status = res.Status,
                CusTags = cusTags,
                CusRegisterTime = res.CusRegisterTime,
                CusSource = res.CusSource,
                StoreId = res.StoreId,
                CusInitialIntegral = res.CusInitialIntegral,
                CusFaceId = res.CusFaceId,
            };

            ret.Success = true;
            ret.ReturnData = retModel;
            ret.Message = "获取成功";
            return ret;
        }

        /// <summary>
        /// 根据编号获取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult<CustomerModel> GetByIdForMsg(string id, UserData ud)
        {
            OperatResult<CustomerModel> ret = new OperatResult<CustomerModel>();
            var res = new Custom().GetById(id);
            if (res == null)
            {
                ret.Success = true;
                ret.Message = "未找到客户信息";
                ret.ReturnData = null;
                return ret;
            }
            ud.MctNum = res.MctNum;
            var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
            List<CustomerLevelModel> levelList = null;
            if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
            {
                levelList = levelPageList.ReturnData.DataList;
            }
            if (levelList == null || levelList.Count == 0)
            {
                ret.Success = true;
                ret.Message = "数据错误，没有配置客户等级信息";
                ret.ReturnData = null;
                return ret;
            }
            CustomerLevelModel levelModel = null;
            if (string.IsNullOrWhiteSpace(res.CusLevelId))
            {
                levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            else
            {
                levelModel = levelList.Find(o => o.Id == res.CusLevelId);
                if (levelModel == null)
                    levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            if (levelModel == null)
                levelModel = new CustomerLevelModel();

            CustomerModel retModel = new CustomerModel()
            {
                BirthdayConsumption = levelModel.BirthdayConsumption,
                CusAccumulatedPoints = res.CusAccumulatedPoints,
                CusAddress = res.CusAddress,
                CusAreaId = res.CusAreaId,
                CusAreaName = res.CusAreaName,
                CusBirthday = res.CusBirthday,
                CusCityId = res.CusCityId,
                CusCityName = res.CusCityName,
                CusCurrentScore = res.CusCurrentScore,
                CusLevelId = levelModel.Id,
                CusLocation = res.CusLocation,
                CusLogo = res.CusLogo,
                CusName = res.CusName,
                CusNo = res.CusNo,
                CusOldMemberNo = res.CusOldMemberNo,
                CusPhoneNo = res.CusPhoneNo,
                CusProvinceId = res.CusProvinceId,
                CusProvinceName = res.CusProvinceName,
                CusRemark = res.CusRemark,
                CusSex = res.CusSex ? "男" : "女",
                CusWechatNo = res.CusWechatNo,
                CusIsBindWX = res.CusIsBindWX, //客户是否已绑定微信
                Id = res.Id,
                IsActivation = res.IsActivation,
                LevelGrade = levelModel.Grade,
                OrdinaryConsumption = levelModel.OrdinaryConsumption,
                OtherEquity = levelModel.OtherEquity,
                ProductOffer = levelModel.ProductOffer,
                Status = res.Status,
                CusRegisterTime = res.CusRegisterTime,
                CusSource = res.CusSource,
                StoreId = res.StoreId,
                CusInitialIntegral = res.CusInitialIntegral,
                CusFaceId = res.CusFaceId,
            };

            ret.Success = true;
            ret.ReturnData = retModel;
            ret.Message = "获取成功";
            return ret;
        }

        /// <summary>
        /// 根据手机号获取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult<CustomerModel> GetShareByPhone(string phone, UserData ud)
        {
            OperatResult<CustomerModel> ret = new OperatResult<CustomerModel>();
            List<string> storeIds = new List<string>();
            var storeList = StoreAndStaffServiceTransfer.GetStoreList(ud.TokenStr); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
            if (storeList != null && storeList.Count > 0)
            {
                var store = storeList.Find(o => o.Id == ud.StoreId);
                if (store == null)
                {
                    storeIds.Add(ud.StoreId); //改组没有查询到自身信息，直接返回自身
                }
                else
                {
                    if (store.IsShareMemberAIntegral)
                    {
                        foreach (var item in storeList)
                        {
                            storeIds.Add(item.Id);
                        }
                    }
                    else
                    {
                        storeIds.Add(ud.StoreId); //门店组没有共享会员信息
                    }
                }
            }
            else
            {
                storeIds.Add(ud.StoreId); //没有查询到门店组信息，直接返回自身
            }
            var res = new Custom().GetByPhone(ud.MctNum, phone, storeIds.ToArray(), "");
            if (res == null || res.MctNum != ud.MctNum)
            {
                ret.Success = true;
                ret.Message = "未找到客户信息";
                ret.ReturnData = null;
                return ret;
            }
            var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
            List<CustomerLevelModel> levelList = null;
            if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
            {
                levelList = levelPageList.ReturnData.DataList;
            }
            if (levelList == null || levelList.Count == 0)
            {
                ret.Success = true;
                ret.Message = "数据错误，没有配置客户等级信息";
                ret.ReturnData = null;
                return ret;
            }
            CustomerLevelModel levelModel = null;
            if (string.IsNullOrWhiteSpace(res.CusLevelId))
            {
                levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            else
            {
                levelModel = levelList.Find(o => o.Id == res.CusLevelId);
                if (levelModel == null)
                    levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            if (levelModel == null)
                levelModel = new CustomerLevelModel();
            List<string> tagStoreIds = new List<string>() { ud.StoreId };
            var chief = StoreAndStaffServiceTransfer.GetChiefByMct(ud.MctNum);
            if (chief != null && chief.Id != ud.StoreId)
            {
                tagStoreIds.Add(chief.Id);
            }
            var tags = new Data.CustomerTags().GetList(res.Id, ud.MctNum, tagStoreIds.ToArray());
            List<WJewel.DataContract.CRM.Customer.CustomerTags> cusTags = new List<WJewel.DataContract.CRM.Customer.CustomerTags>();
            if (tags != null && tags.Count > 0)
            {
                var labels = CustomerLabelService.GetLabelNameDropDown(ud);
                if (labels != null && labels.ReturnData != null)
                {
                    foreach (var item in tags)
                    {
                        var label = labels.ReturnData.Find(o => o.Id == item.LabelId);
                        cusTags.Add(new WJewel.DataContract.CRM.Customer.CustomerTags() { Id = item.Id, Name = label == null ? "" : label.Name, LabelId = item.LabelId });
                    }
                }
            }
            var staffModel = new StaffCustomer().GetInfo(ud.StoreId, res.Id);
            CustomerModel retModel = new CustomerModel()
            {
                BirthdayConsumption = levelModel.BirthdayConsumption,
                CusAccumulatedPoints = res.CusAccumulatedPoints,
                CusAddress = res.CusAddress,
                CusAreaId = res.CusAreaId,
                CusAreaName = res.CusAreaName,
                CusBirthday = res.CusBirthday,
                CusCityId = res.CusCityId,
                CusCityName = res.CusCityName,
                CusCurrentScore = res.CusCurrentScore,
                CusFollowPerson = staffModel != null ? staffModel.StaffId : "",
                CusFollowPersonName = staffModel != null ? staffModel.StaffName : "",
                CusLevelId = levelModel.Id,
                CusLocation = res.CusLocation,
                CusLogo = res.CusLogo,
                CusName = res.CusName,
                CusNo = res.CusNo,
                CusOldMemberNo = res.CusOldMemberNo,
                CusPhoneNo = res.CusPhoneNo,
                CusProvinceId = res.CusProvinceId,
                CusProvinceName = res.CusProvinceName,
                CusRemark = res.CusRemark,
                CusSex = res.CusSex ? "男" : "女",
                CusWechatNo = res.CusWechatNo,
                CusIsBindWX = res.CusIsBindWX,
                Id = res.Id,
                IsActivation = res.IsActivation,
                LevelGrade = levelModel.Grade,
                OrdinaryConsumption = levelModel.OrdinaryConsumption,
                OtherEquity = levelModel.OtherEquity,
                ProductOffer = levelModel.ProductOffer,
                Status = res.Status,
                CusTags = cusTags,
                CusRegisterTime = res.CusRegisterTime,
                CusSource = res.CusSource,
                StoreId = res.StoreId,
                CusInitialIntegral = res.CusInitialIntegral,
                CusFaceId = res.CusFaceId,
            };

            ret.Success = true;
            ret.ReturnData = retModel;
            ret.Message = "获取成功";
            return ret;
        }

        /// <summary>
        /// 根据人脸访客编号获取客户信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult<CustomerModel> GetByFaceId(int faceId, UserData ud)
        {
            OperatResult<CustomerModel> ret = new OperatResult<CustomerModel>();
            List<string> storeIds = new List<string>();
            var storeList = StoreAndStaffServiceTransfer.GetStoreList(ud.TokenStr); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
            if (storeList != null && storeList.Count > 0)
            {
                var store = storeList.Find(o => o.Id == ud.StoreId);
                if (store == null)
                {
                    storeIds.Add(ud.StoreId); //改组没有查询到自身信息，直接返回自身
                }
                else
                {
                    if (store.IsShareMemberAIntegral)
                    {
                        foreach (var item in storeList)
                        {
                            storeIds.Add(item.Id);
                        }
                    }
                    else
                    {
                        storeIds.Add(ud.StoreId); //门店组没有共享会员信息
                    }
                }
            }
            else
            {
                storeIds.Add(ud.StoreId); //没有查询到门店组信息，直接返回自身
            }
            var res = new Custom().GetByFaceId(faceId, ud.MctNum, storeIds.ToArray());
            if (res == null || res.MctNum != ud.MctNum)
            {
                ret.Success = true;
                ret.Message = "未找到客户信息";
                ret.ReturnData = null;
            }

            var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
            List<CustomerLevelModel> levelList = null;
            if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
            {
                levelList = levelPageList.ReturnData.DataList;
            }
            if (levelList == null || levelList.Count == 0)
            {
                ret.Success = true;
                ret.Message = "数据错误，没有配置客户等级信息";
                ret.ReturnData = null;
            }
            CustomerLevelModel levelModel = null;
            if (string.IsNullOrWhiteSpace(res.CusLevelId))
            {
                levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            else
            {
                levelModel = levelList.Find(o => o.Id == res.CusLevelId);
                if (levelModel == null)
                    levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            if (levelModel == null)
                levelModel = new CustomerLevelModel();
            List<string> tagStoreIds = new List<string>() { ud.StoreId };
            var chief = StoreAndStaffServiceTransfer.GetChiefByMct(ud.MctNum);
            if (chief != null && chief.Id != ud.StoreId)
            {
                tagStoreIds.Add(chief.Id);
            }
            var tags = new Data.CustomerTags().GetList(res.Id, ud.MctNum, tagStoreIds.ToArray());
            List<WJewel.DataContract.CRM.Customer.CustomerTags> cusTags = new List<WJewel.DataContract.CRM.Customer.CustomerTags>();
            if (tags != null && tags.Count > 0)
            {
                var labels = CustomerLabelService.GetLabelNameDropDown(ud);
                if (labels != null && labels.ReturnData != null)
                {
                    foreach (var item in tags)
                    {
                        var label = labels.ReturnData.Find(o => o.Id == item.LabelId);
                        cusTags.Add(new WJewel.DataContract.CRM.Customer.CustomerTags() { Id = item.Id, Name = label == null ? "" : label.Name, LabelId = item.LabelId });
                    }
                }
            }
            var staffModel = new StaffCustomer().GetInfo(ud.StoreId, res.Id);
            CustomerModel retModel = new CustomerModel()
            {
                BirthdayConsumption = levelModel.BirthdayConsumption,
                CusAccumulatedPoints = res.CusAccumulatedPoints,
                CusAddress = res.CusAddress,
                CusAreaId = res.CusAreaId,
                CusAreaName = res.CusAreaName,
                CusBirthday = res.CusBirthday,
                CusCityId = res.CusCityId,
                CusCityName = res.CusCityName,
                CusCurrentScore = res.CusCurrentScore,
                CusFollowPerson = staffModel != null ? staffModel.StaffId : "",
                CusFollowPersonName = staffModel != null ? staffModel.StaffName : "",
                CusLevelId = levelModel.Id,
                CusLocation = res.CusLocation,
                CusLogo = res.CusLogo,
                CusName = res.CusName,
                CusNo = res.CusNo,
                CusOldMemberNo = res.CusOldMemberNo,
                CusPhoneNo = res.CusPhoneNo,
                CusProvinceId = res.CusProvinceId,
                CusProvinceName = res.CusProvinceName,
                CusRemark = res.CusRemark,
                CusSex = res.CusSex ? "男" : "女",
                CusWechatNo = res.CusWechatNo,
                CusIsBindWX = res.CusIsBindWX,
                Id = res.Id,
                IsActivation = res.IsActivation,
                LevelGrade = levelModel.Grade,
                OrdinaryConsumption = levelModel.OrdinaryConsumption,
                OtherEquity = levelModel.OtherEquity,
                ProductOffer = levelModel.ProductOffer,
                Status = res.Status,
                CusTags = cusTags,
                CusRegisterTime = res.CusRegisterTime,
                CusSource = res.CusSource,
                CusInitialIntegral = res.CusInitialIntegral,
                CusFaceId = faceId,
            };

            ret.Success = true;
            ret.ReturnData = retModel;
            ret.Message = "获取成功";
            return ret;
        }

        /// <summary>
        /// 根据人脸访客编号获取客户信息
        /// </summary>
        /// <param name="faceIds"></param>
        /// <param name="mctNum"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public static OperatResult<List<CustomerModel>> GetListByFaceIds(int[] faceIds)
        {
            OperatResult<List<CustomerModel>> ret = new OperatResult<List<CustomerModel>>();
            List<CustomerModel> retList = new List<CustomerModel>();

            var customerList = new Custom().GetListByFaceIds(faceIds);
            if (customerList == null || customerList.Count == 0)
            {
                ret.Success = true;
                ret.Message = "未找到客户信息";
                ret.ReturnData = null;
            }

            var levelList = new CustomerLevel().GetAllList();
            if (levelList == null || levelList.Count == 0)
            {
                ret.Success = true;
                ret.Message = "数据错误，没有配置客户等级信息";
                ret.ReturnData = null;
            }

            foreach (var res in customerList)
            {
                Cus_CustomerLevel levelModel = null;
                if (string.IsNullOrWhiteSpace(res.CusLevelId))
                {
                    levelModel = levelList.Find(o => o.Grade == "普通客户" && o.MctNum == res.MctNum && o.IsMainStore == true);
                }
                else
                {
                    levelModel = levelList.Find(o => o.Id == res.CusLevelId);
                    if (levelModel == null)
                        levelModel = levelList.Find(o => o.Grade == "普通客户" && o.MctNum == res.MctNum && o.IsMainStore == true);
                }
                if (levelModel == null)
                    levelModel = new Cus_CustomerLevel();
                List<string> tagStoreIds = new List<string>() { res.StoreId };
                var tags = new Data.CustomerTags().GetList(res.Id, res.MctNum, tagStoreIds.ToArray());
                List<WJewel.DataContract.CRM.Customer.CustomerTags> cusTags = new List<WJewel.DataContract.CRM.Customer.CustomerTags>();
                if (tags != null && tags.Count > 0)
                {
                    var labels = CustomerLabelService.GetLabelNameDropDown(res.MctNum, res.StoreId);
                    if (labels != null && labels.ReturnData != null)
                    {
                        foreach (var item in tags)
                        {
                            var label = labels.ReturnData.Find(o => o.Id == item.LabelId);
                            cusTags.Add(new WJewel.DataContract.CRM.Customer.CustomerTags() { Id = item.Id, Name = label == null ? "" : label.Name, LabelId = item.LabelId });
                        }
                    }
                }
                var staffModel = new StaffCustomer().GetInfo(res.StoreId, res.Id);
                CustomerModel retModel = new CustomerModel()
                {
                    BirthdayConsumption = levelModel.BirthdayConsumption,
                    CusAccumulatedPoints = res.CusAccumulatedPoints,
                    CusAddress = res.CusAddress,
                    CusAreaId = res.CusAreaId,
                    CusAreaName = res.CusAreaName,
                    CusBirthday = res.CusBirthday,
                    CusCityId = res.CusCityId,
                    CusCityName = res.CusCityName,
                    CusCurrentScore = res.CusCurrentScore,
                    CusFollowPerson = staffModel != null ? staffModel.StaffId : "",
                    CusFollowPersonName = staffModel != null ? staffModel.StaffName : "",
                    CusLevelId = levelModel.Id,
                    CusLocation = res.CusLocation,
                    CusLogo = res.CusLogo,
                    CusName = res.CusName,
                    CusNo = res.CusNo,
                    CusOldMemberNo = res.CusOldMemberNo,
                    CusPhoneNo = res.CusPhoneNo,
                    CusProvinceId = res.CusProvinceId,
                    CusProvinceName = res.CusProvinceName,
                    CusRemark = res.CusRemark,
                    CusSex = res.CusSex ? "男" : "女",
                    //CusWechatNo = string.IsNullOrWhiteSpace(res.CusWechatNo) ? "未绑定" : res.CusWechatNo,
                    CusWechatNo = res.CusWechatNo,
                    CusIsBindWX = res.CusIsBindWX,
                    Id = res.Id,
                    IsActivation = res.IsActivation,
                    LevelGrade = levelModel.Grade,
                    OrdinaryConsumption = levelModel.OrdinaryConsumption,
                    OtherEquity = levelModel.OtherEquity,
                    ProductOffer = levelModel.ProductOffer,
                    Status = res.Status,
                    CusTags = cusTags,
                    CusRegisterTime = res.CusRegisterTime,
                    CusSource = res.CusSource,
                    CusFaceId = res.CusFaceId,
                    CusInitialIntegral = res.CusInitialIntegral,
                };
                retList.Add(retModel);
            }

            ret.Success = true;
            ret.ReturnData = retList;
            ret.Message = "获取成功";
            return ret;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult ModifyStatus(string id, UserData ud)
        {
            OperatResult ret = new OperatResult();
            var res = new Custom().GetById(id);
            if (res == null || res.MctNum != ud.MctNum)
            {
                ret.Success = false;
                ret.Message = "未找到客户信息";
            }
            if (new Custom().ModifyStatus(id, !res.Status))
            {
                ret.Success = true;
                ret.Message = "更新成功";
            }
            else
            {
                ret.Success = false;
                ret.Message = "更新失败";
            }
            return ret;
        }


        /// <summary>
        /// 客户进店分配跟进人,已存在跟进人，默认返回true
        /// </summary>
        public static OperatResult AssignmentFollowers(string cusId, string staffId, UserData ud)
        {
            OperatResult res = new OperatResult();
            var staff = StoreAndStaffServiceTransfer.GetStaffDetailInfo(staffId);
            if (staff != null)
            {
                var staffCus = new StaffCustomer().GetInfo(staff.StoreId, cusId);
                if (staffCus != null)
                {
                    if (string.IsNullOrWhiteSpace(staffCus.StaffId))
                    {
                        staffCus.StaffId = staff.Id;
                        staffCus.StaffName = staff.StaffName;
                        if (new StaffCustomer().Modify(staffCus))
                        {
                            res.Success = true;
                            res.Message = "跟进人已更新";
                        }
                        else
                        {
                            res.Success = false;
                            res.Message = "更新失败";
                        }
                    }
                    else
                    {
                        res.Success = true;
                        res.Message = "已存在跟进人";
                    }
                }
                else
                {
                    Cus_StaffCustomer create = new Cus_StaffCustomer()
                    {
                        CustomerId = cusId,
                        DistributionId = "",
                        DistributionTime = DateTime.Now,
                        Id = SecureHelper.GetNum(),
                        MctNum = ud.MctNum,
                        StoreId = staff.StoreId,
                        StaffId = staff.Id,
                        StaffName = staff.StaffName,
                    };
                    if (new StaffCustomer().Create(new List<Cus_StaffCustomer>() { create }))
                    {
                        res.Success = true;
                        res.Message = "跟进人已更新";
                    }
                    else
                    {
                        res.Success = false;
                        res.Message = "更新失败";
                    }
                }
            }
            else
            {
                res.Success = false;
                res.Message = "未找到员工信息";
            }
            return res;
        }

        /// <summary>
        /// 获取未跟进客户统计
        /// </summary>
        /// <param name="ud"></param>
        /// <returns></returns>
        public static OperatResult<List<NoFollowCustomStatistics>> GetStatistics(UserData ud)
        {
            OperatResult<List<NoFollowCustomStatistics>> res = new OperatResult<List<NoFollowCustomStatistics>>();
            List<NoFollowCustomStatistics> resList = new List<NoFollowCustomStatistics>();
            var redis = new RedisHelper();
            if (redis != null)
            {
                var temp = redis.HashKeys(RedisPrimaryKey.STAFFCUSTOMERNUMBER + ud.StoreId);
                if (temp != null && temp.Count > 0)
                {
                    if (temp.FirstOrDefault() != ud.UserID)
                    {
                        var staff = StoreAndStaffServiceTransfer.GetStoreStaffInfo(temp.FirstOrDefault(), ud.TokenStr);
                        if (staff == null)
                        {
                            res.Success = false;
                            res.Message = "已有员工在分配客户，数据已锁定，请等半小时后再试";
                            res.ReturnData = null;
                        }
                        else
                        {
                            res.Success = false;
                            res.Message = staff.StaffName + " 正在分配客户，如需分配请联系该员工，或者半小时后再试";
                            res.ReturnData = null;
                        }
                        return res;
                    }
                }
            }
            var resLevel = CustomerLevelService.GetDropDownList(ud);
            if (resLevel != null && resLevel.ReturnData != null && resLevel.ReturnData.Count > 0)
            {
                var level = resLevel.ReturnData.Find(o => o.Name == "游客");
                if (level != null)
                {
                    resLevel.ReturnData.Remove(level);
                }
            }
            else
            {
                NoFollowCustomStatistics Statistics = new NoFollowCustomStatistics()
                {
                    Count = 0,
                    IsTotal = true,
                    Name = "合计",
                };
                resList.Add(Statistics);

                res.Success = true;
                res.Message = "没有会员等级";
                res.ReturnData = resList;
                return res;
            }
            var staffCus = new StaffCustomer().GetList(ud.StoreId);
            List<string> cusIds = staffCus.FindAll(o => string.IsNullOrWhiteSpace(o.StaffId)).Select(o => o.CustomerId).ToList();
            if (cusIds == null || cusIds.Count == 0)
            {
                NoFollowCustomStatistics Statistics = new NoFollowCustomStatistics()
                {
                    Count = 0,
                    IsTotal = true,
                    Name = "合计",
                };
                resList.Add(Statistics);

                res.Success = true;
                res.Message = "没有未跟进客户";
                res.ReturnData = resList;
                return res;
            }
            List<string> storeIds = new List<string>();
            var storeList = StoreAndStaffServiceTransfer.GetStoreList(ud.TokenStr); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
            if (storeList != null && storeList.Count > 0)
            {
                var store = storeList.Find(o => o.Id == ud.StoreId);
                if (store == null)
                {
                    storeIds.Add(ud.StoreId); //改组没有查询到自身信息，直接返回自身
                }
                else
                {
                    if (store.IsShareMemberAIntegral)
                    {
                        foreach (var item in storeList)
                        {
                            storeIds.Add(item.Id);
                        }
                    }
                    else
                    {
                        storeIds.Add(ud.StoreId); //门店组没有共享会员信息
                    }
                }
            }
            else
            {
                storeIds.Add(ud.StoreId); //没有查询到门店组信息，直接返回自身
            }
            var ret = new Custom().GetList(ud.MctNum, storeIds.ToArray(), cusIds.ToArray());
            if (ret != null && ret.Count > 0)
            {
                foreach (var level in resLevel.ReturnData)
                {
                    List<Cus_Customer> tempList = new List<Cus_Customer>();
                    if (level.Name == "普通客户")
                    {
                        tempList = ret.FindAll(o => o.CusLevelId == level.Id || string.IsNullOrEmpty(o.CusLevelId));
                    }
                    else
                    {
                        tempList = ret.FindAll(o => o.CusLevelId == level.Id);
                    }
                    NoFollowCustomStatistics model = new NoFollowCustomStatistics()
                    {
                        Count = tempList.Count,
                        CustomerIds = tempList != null ? tempList.Select(o => o.Id).ToList() : new List<string>(),
                        IsTotal = false,
                        Name = level.Name,
                    };
                    resList.Add(model);
                }
                NoFollowCustomStatistics Statistics = new NoFollowCustomStatistics()
                {
                    Count = resList.Sum(o => o.Count),
                    CustomerIds = new List<string>(),
                    IsTotal = true,
                    Name = "合计",
                };
                resList.Add(Statistics);
                res.Success = true;
                res.Message = "获取成功";
                res.ReturnData = resList;

                if (redis != null)
                {
                    redis.HashSet<List<NoFollowCustomStatistics>>(RedisPrimaryKey.STAFFCUSTOMERNUMBER + ud.StoreId, ud.UserID, resList, GetTomorrowZeroTime(DateTime.Now));
                }
            }
            else
            {
                foreach (var level in resLevel.ReturnData)
                {
                    NoFollowCustomStatistics model = new NoFollowCustomStatistics()
                    {
                        Count = 0,
                        IsTotal = false,
                        Name = level.Name,
                    };
                    resList.Add(model);
                }
                NoFollowCustomStatistics Statistics = new NoFollowCustomStatistics()
                {
                    Count = resList.Sum(o => o.Count),
                    IsTotal = true,
                    Name = "合计",
                };
                resList.Add(Statistics);

                res.Success = true;
                res.Message = "没有客户数据";
                res.ReturnData = resList;
            }
            return res;
        }

        /// <summary>
        /// 员工客户分配
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static OperatResult StaffCustomerDis(List<StaffCustomerNumber> list, UserData ud)
        {
            OperatResult ret = new OperatResult();
            if (list != null && list.Count > 0)
            {
                var redis = new RedisHelper();
                if (redis != null)
                {
                    var customerNumber = redis.HashGet<List<NoFollowCustomStatistics>>(RedisPrimaryKey.STAFFCUSTOMERNUMBER + ud.StoreId, ud.UserID);
                    if (customerNumber != null && customerNumber.Count > 0)
                    {
                        customerNumber = customerNumber.FindAll(o => o.CustomerIds != null && o.CustomerIds.Count > 0);
                        List<Cus_StaffCustomer> resList = new List<Cus_StaffCustomer>();
                        var totalCount = 0;
                        var cusList = new Custom().GetList(ud.MctNum, ref totalCount, new string[] { ud.StoreId }, ud.StoreId, "", false, "", "", "", "", "", "", -1, -1, 1, 100000);
                        if (cusList != null && cusList.Count > 0)
                        {
                            var staffList = StoreAndStaffServiceTransfer.GetDropList(ud.TokenStr);
                            for (int i = 0; i < list.Max(o => o.CustomerNumber); i++)  //员工最多客户数量
                            {
                                for (int j = 0; j < list.Count; j++)  //循环员工
                                {
                                    if (resList.FindAll(o => o.StaffId == list[j].StaffId).Count >= list[j].CustomerNumber) //员工已经到达分配数直接跳过
                                    {
                                        continue;
                                    }
                                    var staff = staffList.Find(o => o.Id == list[j].StaffId);
                                    foreach (var item in customerNumber)  //未跟进人客户
                                    {
                                        if (item.CustomerIds == null || item.CustomerIds.Count == 0)  //如果等级未跟进客户已经分配完，到下一个等级
                                        {
                                            continue;
                                        }
                                        if (resList.FindAll(o => o.StaffId == list[j].StaffId).Count >= list[j].CustomerNumber) //员工已经到达分配数直接跳过
                                        {
                                            continue;
                                        }
                                        Cus_StaffCustomer model = new Cus_StaffCustomer()
                                        {
                                            CustomerId = item.CustomerIds.FirstOrDefault(),
                                            DistributionId = ud.UserID,
                                            DistributionTime = DateTime.Now,
                                            Id = SecureHelper.GetNum(),
                                            StaffId = list[j].StaffId,
                                            MctNum = ud.MctNum,
                                            StaffName = staff != null ? staff.StaffName : "",
                                            StoreId = ud.StoreId,
                                        };
                                        resList.Add(model);
                                        item.CustomerIds.Remove(item.CustomerIds.FirstOrDefault());
                                        break;
                                    }
                                }
                            }
                            if (new StaffCustomer().Create(resList))
                            {
                                redis.HashDelete(RedisPrimaryKey.STAFFCUSTOMERNUMBER + ud.StoreId, ud.UserID);
                                ret.Success = true;
                                ret.Message = "分配完成";
                            }
                            else
                            {
                                ret.Success = false;
                                ret.Message = "分配失败";
                            }
                        }
                        else
                        {
                            ret.Success = false;
                            ret.Message = "找不到客户数据";
                        }
                    }
                    else
                    {
                        ret.Success = false;
                        ret.Message = "数据已失效，请关闭分配窗口重新点开获取最新数据";
                    }
                }
            }
            return ret;
        }

        #region 微信H5使用
        /// <summary>
        /// 微信获取会员客户信息-微信H5使用
        /// </summary>
        /// <param name="ud">当前登录用户的信息</param>
        /// <param name="mctNum">商户号</param>
        /// <returns></returns>
        public static OperatResult<CustomerUserForWXModel> GetCustomerUserInfoForWX(UserData ud, string mctNum, string storeId = "")
        {
            OperatResult<CustomerUserForWXModel> ret = new OperatResult<CustomerUserForWXModel>();

            CustomerUserForWXModel retModel = new CustomerUserForWXModel();
            ud.MctNum = mctNum;
            List<string> storeIds = new List<string>();
            var storeList = StoreAndStaffServiceTransfer.GetStoreListNoToken(storeId); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
            if (storeList != null && storeList.Count > 0)
            {
                var store = storeList.Find(o => o.Id == storeId);
                if (store == null)
                {
                    if (!string.IsNullOrEmpty(storeId))
                        storeIds.Add(storeId); //改组没有查询到自身信息，直接返回自身
                }
                else
                {
                    if (store.IsShareMemberAIntegral)
                    {
                        foreach (var item in storeList)
                        {
                            storeIds.Add(item.Id);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(storeId))
                            storeIds.Add(storeId); //门店组没有共享会员信息
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(storeId))
                    storeIds.Add(storeId); //没有查询到门店组信息，直接返回自身
            }
            //找到用户信息,然后使用手机号找到客户信息
            Cus_Customer customer = null;
            var userInfo = UserCenterServiceTransfer.GetUserBaseInfo(ud.UserID, ud.TokenStr);
            if (userInfo != null && !string.IsNullOrEmpty(userInfo.Mobile))
            {
                customer = new Custom().GetByPhone(mctNum, userInfo.Mobile, storeIds.ToArray());
            }
            if (customer == null)
            {
                ret.Success = true;
                ret.ReturnData = null;
                ret.Message = "暂无数据";
                return ret;
            }

            var levelPageList = CustomerLevelService.GetList(mctNum, string.Empty, 1, 10000);
            List<CustomerLevelModel> levelList = null;
            if (levelPageList != null && levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
            {
                levelList = levelPageList.ReturnData.DataList;
            }
            if (levelList == null || levelList.Count == 0)
            {
                ret.Success = true;
                ret.Message = "暂无数据";
                ret.ReturnData = null;
                return ret;
            }
            CustomerLevelModel levelModel = null;
            if (string.IsNullOrWhiteSpace(customer.CusLevelId))
            {
                levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            else
            {
                levelModel = levelList.Find(o => o.Id == customer.CusLevelId);
                if (levelModel == null)
                    levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            if (levelModel == null)
                levelModel = new CustomerLevelModel();

            //生日
            DateTime birthday = DateTime.Now;
            if (customer.CusBirthday != null)
                DateTime.TryParse(customer.CusBirthday.ToString(), out birthday);

            string weiXinDefaultHeadImg = ConfigHelper.GetAppSetting("WeiXinDefaultHeadImg");
            retModel = new CustomerUserForWXModel()
            {
                Id = customer.Id,
                StoreId = customer.StoreId,
                CusCurrentScore = (decimal)customer.CusCurrentScore,
                CusLevelId = levelModel.Id,
                CusLevelName = levelModel.Grade,
                CusName = customer.CusName,
                CusSex = customer.CusSex ? "男" : "女",
                IsActivation = customer.IsActivation,
                CusPhoneNo = customer.CusPhoneNo,
                CusLogo = string.IsNullOrEmpty(customer.CusWxLogo) ? weiXinDefaultHeadImg : customer.CusWxLogo,
                CusNo = customer.CusNo,
                CusBirthday = birthday.ToString("yyyy-MM-dd")
            };
            ret.ReturnData = retModel;

            return ret;
        }

        /// <summary>
        /// 微信获取会员客户信息-微信H5使用
        /// </summary>
        /// <param name="ud">当前登录用户的信息</param>
        /// <param name="mctNum">商户号</param>
        /// <returns></returns>
        public static OperatResult<CustomerUserForWXModel> GetCustomerUserInfoByMobileForWX(string mobile, string storeId = "")
        {
            OperatResult<CustomerUserForWXModel> ret = new OperatResult<CustomerUserForWXModel>();

            CustomerUserForWXModel retModel = new CustomerUserForWXModel();
            //找到用户信息,然后使用手机号找到客户信息
            Cus_Customer customer = CustomerService.GetCustomerByUser(mobile, storeId);
            if (customer == null)
            {
                ret.Success = true;
                ret.ReturnData = null;
                ret.Message = "暂无数据";
                return ret;
            }

            var levelPageList = CustomerLevelService.GetList(customer.MctNum, string.Empty, 1, 10000);
            List<CustomerLevelModel> levelList = null;
            if (levelPageList != null && levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
            {
                levelList = levelPageList.ReturnData.DataList;
            }
            if (levelList == null || levelList.Count == 0)
            {
                ret.Success = true;
                ret.Message = "暂无数据";
                ret.ReturnData = null;
                return ret;
            }
            CustomerLevelModel levelModel = null;
            if (string.IsNullOrWhiteSpace(customer.CusLevelId))
            {
                levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            else
            {
                levelModel = levelList.Find(o => o.Id == customer.CusLevelId);
                if (levelModel == null)
                    levelModel = levelList.Find(o => o.Grade == "普通客户");
            }
            if (levelModel == null)
                levelModel = new CustomerLevelModel();

            string weiXinDefaultHeadImg = ConfigHelper.GetAppSetting("WeiXinDefaultHeadImg");
            retModel = new CustomerUserForWXModel()
            {
                Id = customer.Id,
                CusCurrentScore = (decimal)customer.CusCurrentScore,
                CusLevelId = levelModel.Id,
                CusLevelName = levelModel.Grade,
                CusName = customer.CusName,
                CusSex = customer.CusSex ? "男" : "女",
                IsActivation = customer.IsActivation,
                CusPhoneNo = customer.CusPhoneNo,
                CusLogo = string.IsNullOrEmpty(customer.CusWxLogo) ? weiXinDefaultHeadImg : customer.CusWxLogo,
                CusNo = customer.CusNo,
                CusBirthday = customer.CusBirthday != null ? (ValidateHelper.IsDate(customer.CusBirthday.Tostring()) ? Convert.ToDateTime(customer.CusBirthday).ToString("yyyy-MM-dd") : string.Empty) : string.Empty
            };
            ret.ReturnData = retModel;

            return ret;
        }

        /// <summary>
        /// 微信更新会员客户信息-微信H5使用   (需求确认客户修改自己手机号后修改他自己自身用户的手机号。平台下所有该号码的更新)
        /// </summary>
        /// <param name="ud">当前登录用户的信息</param>
        /// <param name="mctNum">商户号</param>
        /// <returns></returns>
        public static OperatResult<bool> ModifyForWX(UserData ud, CustomerUserModifyForWXModel model, string mctNum)
        {
            OperatResult<bool> ret = new OperatResult<bool>();

            Cus_Customer customer = null;
            if (!string.IsNullOrEmpty(model.Id))
            {
                //客户信息
                customer = new Custom().GetById(model.Id);
                if (customer == null)
                {
                    ret.Success = false;
                    ret.ReturnData = false;
                    ret.Message = "修改信息失败,无效的用户";
                    return ret;
                }
            }
            if (customer == null)
            {
                //找到用户信息,然后使用手机号找到客户信息
                customer = CustomerService.GetCustomerByUser(ud, mctNum);
                if (customer == null)
                {
                    ret.Success = false;
                    ret.ReturnData = false;
                    ret.Message = "修改信息失败,请先绑定微信公众号";
                    return ret;
                }
            }
            bool dbRet = false;
            switch (model.OperateType)
            {
                case 1: //修改头像
                    if (string.IsNullOrEmpty(model.CusLogo))
                    {
                        ret.Success = false;
                        ret.ReturnData = false;
                        ret.Message = "修改信息失败,请上传头像";
                        return ret;
                    }
                    dbRet = new Custom().ModifyLogo(mctNum, customer.Id, model.CusLogo);
                    break;
                case 2: //修改姓名
                    if (string.IsNullOrEmpty(model.CusName))
                    {
                        ret.Success = false;
                        ret.ReturnData = false;
                        ret.Message = "修改信息失败,请填写姓名";
                        return ret;
                    }
                    customer.CusName = model.CusName;
                    dbRet = new Custom().ModifyName(mctNum, customer.Id, model.CusName);
                    break;
                case 3: //修改手机
                    if (string.IsNullOrEmpty(model.CusPhoneNo))
                    {
                        ret.Success = false;
                        ret.ReturnData = false;
                        ret.Message = "修改信息失败,请填写新手机号";
                        return ret;
                    }
                    if (string.IsNullOrEmpty(model.PhoneCheckCode))
                    {
                        ret.Success = false;
                        ret.ReturnData = false;
                        ret.Message = "修改信息失败,请填写手机验证码";
                        return ret;
                    }
                    var smsCode = RedisService.Instance.HashGet<string>(RedisPrimaryKey.RESETPHONENO, model.CusPhoneNo);
                    if (string.IsNullOrWhiteSpace(smsCode))
                    {
                        ret.Success = false;
                        ret.Message = "验证码已失效";
                        return ret;
                    }
                    else
                    {
                        if (smsCode != model.PhoneCheckCode)
                        {
                            ret.Success = false;
                            ret.Message = "验证码输入错误";
                            return ret;
                        }
                    }
                    customer.CusPhoneNo = model.CusPhoneNo;
                    var updateCRMUserMobileRet = UserCenterServiceTransfer.UpdateCRMUserMobile(ud.TokenStr, model.CusPhoneNo);
                    if (updateCRMUserMobileRet)
                    {
                        dbRet = new Custom().ModifyPhone(model.CusPhoneNo);
                    }
                    break;
                case 4: //修改性别
                    dbRet = new Custom().ModifySex(mctNum, customer.Id, model.CusSex);
                    break;
                case 5: //修改生日
                    if (string.IsNullOrEmpty(model.CusBirthday))
                    {
                        ret.Success = false;
                        ret.ReturnData = false;
                        ret.Message = "修改信息失败,请填写生日";
                        return ret;
                    }
                    if (!ValidateHelper.IsDate(model.CusBirthday))
                    {
                        ret.Success = false;
                        ret.ReturnData = false;
                        ret.Message = "修改信息失败,请填写有效的生日";
                        return ret;
                    }
                    dbRet = new Custom().ModifyBirthday(mctNum, customer.Id, model.CusBirthday.ToDateTime());
                    break;
                default:
                    break;
            }

            if (!dbRet)
            {
                if (model.OperateType == 2 || model.OperateType == 3)
                {
                    SyncErp(customer);
                }
                ret.Success = false;
                ret.ReturnData = false;
                ret.Message = "修改信息失败,操作数据出错";
                return ret;
            }

            ret.Success = true;
            ret.ReturnData = true;
            return ret;
        }

        /// <summary>
        /// 微信更新会员客户信息-微信H5使用   (需求确认客户修改自己手机号后修改他自己自身用户的手机号。平台下所有该号码的更新)
        /// </summary>
        /// <param name="ud">当前登录用户的信息</param>
        /// <param name="mctNum">商户号</param>
        /// <returns></returns>
        public static OperatResult<bool> ModifyCardForWX(CustomerModifyCardNoModel model)
        {
            OperatResult<bool> ret = new OperatResult<bool>();

            Cus_Customer customer = null;
            if (!string.IsNullOrEmpty(model.CusPhoneNo))
            {
                //客户信息
                customer = new Custom().GetByPhone(model.CusPhoneNo, model.StoreId);
                if (customer == null)
                {
                    ret.Success = false;
                    ret.ReturnData = false;
                    ret.Message = "修改信息失败,无效的用户";
                    return ret;
                }
            }
            bool dbRet = false;

            if (string.IsNullOrEmpty(model.CusPhoneNo))
            {
                ret.Success = false;
                ret.ReturnData = false;
                ret.Message = "修改信息失败,请填写手机号";
                return ret;
            }
            customer.CusNo = model.vipCardNo;
            dbRet = new Custom().ModifyOldMemberNo(model.MctNum, customer.Id, model.vipCardNo);

            if (!dbRet)
            {
                ret.Success = false;
                ret.ReturnData = false;
                ret.Message = "修改信息失败,操作数据出错";
                return ret;
            }

            SyncErp(customer);
            ret.Success = true;
            ret.ReturnData = true;
            return ret;
        }

        /// <summary>
        /// 微信会员卡激活后激活客户信息
        /// </summary>
        /// <param name="model">激活客户信息实体</param>
        /// <returns></returns>
        public static OperatResult<string> ActivityForWX(ActivityCustomerForWXModel model)
        {
            OperatResult<string> res = new OperatResult<string>();

            LogHelper.WriteSysFileLog("CustomerService", "ActivityForWX", "", "System", "标题", $"ActivityCustomerForWXModel：{JSONHelper.Encode<ActivityCustomerForWXModel>(model)}", "");

            //初始化积分
            decimal? cusInitialIntegral = 0;
            OperatResult<List<IntegralRulesModel>> integralRulesServiceRet = IntegralRulesService.GetList(model.MctNum);
            if (integralRulesServiceRet != null && integralRulesServiceRet.ReturnData != null && integralRulesServiceRet.ReturnData.Count > 0)
            {
                if (integralRulesServiceRet.ReturnData.Find(s => s.Status == true && s.IntegralRuleId == WJewel.DataContract.CRM.Common.CommonIntegralRuleModel.AddForRegister) != null)
                {
                    cusInitialIntegral = integralRulesServiceRet.ReturnData.Find(s => s.Status == true && s.IntegralRuleId == WJewel.DataContract.CRM.Common.CommonIntegralRuleModel.AddForRegister).IntegralMechanism;
                }
            }

            //是否已存在的客户
            Cus_Customer customer = new Custom().GetByPhone(model.PhoneNo, model.StoreId);
            if (customer == null)
            {
                Cus_Customer create = new Cus_Customer()
                {
                    Id = SecureHelper.GetNum(),
                    CreateAccount = "system",
                    CreateTime = DateTime.Now,
                    CreateUserId = "system",
                    CusBirthday = model.Birthday,
                    CusNo = model.CardCode,// GetCusNo(model.MctCode, model.MctNum),
                    CusName = model.Name,
                    CusPhoneNo = model.PhoneNo,
                    CusRegisterTime = DateTime.Now,
                    CusRemark = "微信会员卡激活",
                    CusSex = model.Gender == 0 ? false : true,
                    LastModifyAccount = "system",
                    LastModifyTime = DateTime.Now,
                    LastModifyUserId = "system",
                    MctNum = model.MctNum,
                    Status = true,
                    CusSource = "微信注册",
                    IsActivation = false, //需通过人脸激活,微信无激活数据
                    StoreId = model.StoreId,
                    IsDelete = false,
                    CusIsBindWX = true,
                    CusInitialIntegral = (decimal)cusInitialIntegral,
                    CusCurrentScore = (decimal)cusInitialIntegral,
                    CusAccumulatedPoints = (decimal)cusInitialIntegral,
                };
                var level = GetCustomerLevel((decimal)create.CusAccumulatedPoints, model.MctNum);
                if (level != null && level.Success && level.ReturnData != null)
                {
                    create.CusLevelId = level.ReturnData.Id;
                }
                List<Cus_IntegralRecord> integralList = new List<Cus_IntegralRecord>();
                if (cusInitialIntegral > 0)
                {
                    Cus_IntegralRecord integralRecord = new Cus_IntegralRecord()
                    {
                        Id = SecureHelper.GetNum(),
                        StoreId = model.StoreId,
                        ERPOrderNo = null,
                        ChangeType = 1,
                        AffectedNumber = (decimal)create.CusCurrentScore,
                        AffectedMoney = 0,
                        ScoreBalance = (decimal)create.CusCurrentScore,
                        BusinessStoreId = model.StoreId,
                        BusinessStaffId = "0",
                        IntegralRulesId = WJewel.DataContract.CRM.Common.CommonIntegralRuleModel.AddForRegister,
                        IntegralType = WJewel.DataContract.CRM.Common.CommonIntegralTypeModel.AddForTask,
                        MainSalerId = null,
                        MctNum = model.MctNum,
                        Remark = "注册初始化积分",
                        CustomerId = create.Id,
                        CreatedDate = DateTime.Now,
                    };
                    integralList.Add(integralRecord);
                }

                List<Cus_BaseCustomer> baseCustoms = new List<Cus_BaseCustomer>();
                var baseCustom = new Custom().GetBaseCustomByPhone(create.CusPhoneNo);
                if (baseCustom == null)
                {
                    baseCustom = new Cus_BaseCustomer()
                    {
                        ActivationTime = new DateTime(),
                        CusPhoneNo = create.CusPhoneNo,
                        Id = SecureHelper.GetNum(),
                        IsActivation = false,
                    };
                    baseCustoms.Add(baseCustom);
                }
                else
                {
                    baseCustoms = null;
                }

                List<Cus_StaffCustomer> staffList = new List<Cus_StaffCustomer>();
                Cus_StaffCustomer staff = new Cus_StaffCustomer()
                {
                    CustomerId = create.Id,
                    DistributionId = "",
                    DistributionTime = DateTime.Now,
                    Id = SecureHelper.GetNum(),
                    MctNum = model.MctNum,
                    StaffId = "",
                    StaffName = "",
                    StoreId = model.StoreId,
                };
                staffList.Add(staff);

                if (new Custom().Create(new List<Cus_Customer>() { create }, null, integralList, staffList, baseCustoms))
                {
                    LogHelper.WriteSysFileLog("Customerservice", "微信会员激活成功", "", "System", "标题", $"成功:{JSONHelper.Encode<ActivityCustomerForWXModel>(model)}", "");
                    res.Success = true;
                    res.ReturnData = create.Id;
                    res.Message = "激活成功";
                    //发送通知
                    new Task(() =>
                    {
                        GenerateTaskMessage(create, MessageTemplateEnum.Cus_Register, model.OpenId);
                    }).Start();
                    // 同步数据
                    LogHelper.WriteSysFileLog("Customerservice", "会员新增同步", "", "System", "标题", $"{JSONHelper.Encode<Cus_Customer>(create)}", "");
                    SyncErp(create);
                }
                else
                {
                    LogHelper.WriteSysFileLog("Customerservice", "微信会员激活失败", "", "System", "标题", $"失败:{JSONHelper.Encode<ActivityCustomerForWXModel>(model)}", "");
                    res.Success = false;
                    res.Message = "激活失败,操作数据出错";
                }
            }
            else
            {
                // 同步数据
                LogHelper.WriteSysFileLog("Customerservice", "已有会员同步", "", "System", "标题", $"{JSONHelper.Encode<Cus_Customer>(customer)}", "");
                SyncErp(customer);
                res.Success = true;
                res.ReturnData = customer.Id;
                res.Message = "激活成功";
            }
            return res;
        }

        /// <summary>
        /// 微信会员更新手机号前的验证
        /// </summary>
        /// <param name="ud">当前登录用户信息</param>
        /// <param name="mobile">新手机号</param>
        /// <returns></returns>
        public static OperatResult<bool> CheckModifyMobile(UserData ud, string phoneNo)
        {
            OperatResult<bool> res = new OperatResult<bool>();
            try
            {
                Cus_Customer customer = CustomerService.GetCustomerByUser(ud, ud.MctNum);
                if (customer.CusPhoneNo == phoneNo)
                {
                    res.Success = true;
                    res.ReturnData = false;
                    res.Message = "新手机号不能与现在的手机号相同";
                    return res;
                }

                customer = new Custom().GetByPhone(phoneNo, string.Empty);
                if (customer != null)
                {
                    res.Success = true;
                    res.ReturnData = false;
                    res.Message = "手机号已被使用";
                    return res;
                }
                res.Success = true;
                res.ReturnData = true;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ReturnData = false;
                res.Message = "手机号已被使用";
            }
            return res;
        }

        /// <summary>
        /// 根据登录用户获取客户信息
        /// </summary>
        /// <param name="ud">当前登录用户信息</param>
        /// <returns></returns>
        public static Cus_Customer GetCustomerByUser(UserData ud, string mctNum, string storeId = "")
        {
            //找到用户信息,然后使用手机号找到客户信息
            Cus_Customer customer = null;
            var userInfo = UserCenterServiceTransfer.GetUserBaseInfo(ud.UserID, ud.TokenStr);
            if (userInfo != null && !string.IsNullOrEmpty(userInfo.Mobile))
            {
                customer = new Custom().GetByMctAndPhone(mctNum, userInfo.Mobile, storeId);
            }
            return customer;
        }

        /// <summary>
        /// 根据登录用户获取客户信息
        /// </summary>
        /// <param name="ud">当前登录用户信息</param>
        /// <returns></returns>
        public static Cus_Customer GetCustomerByUser(string mobile, string storeId = "")
        {
            //找到用户信息,然后使用手机号找到客户信息
            Cus_Customer customer = new Custom().GetByPhone(mobile, storeId);
            return customer;
        }
        #endregion

        /// <summary>
        /// 获取客户会员等级权重
        /// </summary>
        public static OperatResult<CustomerInfo> IsCanPullCardRoll(UserData ud, PullCardRollModel model)
        {
            var result = new OperatResult<CustomerInfo>();
            if ((model.CusLevelIds == null || !model.CusLevelIds.Any()) && (model.CusTagIds == null || !model.CusTagIds.Any()) && (model.cusIds == null || !model.cusIds.Any()))
            {
                result.Success = false;
                result.Message = "客户等级和客户标签或者客户编号不能同时为空";
                return result;
            }
            ud.StoreId = "";
            var AllStoreList = StoreAndStaffServiceTransfer.GetStoreListByMctNums(new GetStoreListByMctNums() { MctNums = new string[] { ud.MctNum } });

            List<Cus_CustomerLevel> levelList = null;
            if (model.CusLevelIds != null && model.CusLevelIds.Any())
            {
                var chief = StoreAndStaffServiceTransfer.GetChiefByMct(ud.MctNum);
                if (chief == null)
                {
                    result.Message = "找不到总部信息";
                    result.Success = false;
                    return result;
                }
                levelList = new CustomerLevel().GetList(ud.MctNum, chief.Id);
                if (levelList == null || !levelList.Any())
                {
                    result.Success = false;
                    result.Message = "商户还未配置客户等级";
                    return result;
                }
                var hasLevelList = levelList.FindAll(s => model.CusLevelIds.Contains(s.Id));
                if (hasLevelList == null || !hasLevelList.Any())
                {
                    result.Success = false;
                    result.Message = "客户等级数据已失效";
                    return result;
                }
            }
            if (model.CusTagIds != null && model.CusTagIds.Any())
            {
                var tagList = new Data.CustomerTags().GetTags(model.CusTagIds.ToArray());
                if (tagList == null || !tagList.Any())
                {
                    result.Success = false;
                    result.Message = "客户标签数据已失效";
                    return result;
                }
            }

            var storeIds = AllStoreList.Select(o => o.Id).ToList();
            var cusList = new Custom().GetListByPhone(new string[] { model.Mobile }, storeIds.ToArray());
            if (cusList == null || !cusList.Any())
            {
                result.Success = false;
                result.Message = "客户数据不存在";
                return result;
            }
            else
            {
                if (levelList != null && levelList.Count > 0)
                {
                    foreach (var item in cusList)
                    {
                        if (string.IsNullOrWhiteSpace(item.CusLevelId))
                        {
                            var level = levelList.Find(o => o.Grade == "普通客户");
                            if (level != null)
                            {
                                item.CusLevelId = level.Id;
                            }
                        }
                    }
                }
            }
            Cus_Customer cus = new Cus_Customer();
            if (model.CusLevelIds != null && model.CusLevelIds.Any())
            {
                var cList = cusList.FindAll(s => model.CusLevelIds.Contains(s.CusLevelId));
                if (cList != null && cList.Count > 0 && model.RedeemPoints > 0)
                {
                    cus = cList.Find(o => o.CusCurrentScore > model.RedeemPoints);
                    if (cus == null)
                    {
                        result.Success = false;
                        result.Message = "积分不足";
                        return result;
                    }
                }
                else
                {
                    cus = cList.FirstOrDefault();
                }
            }

            if (model.CusTagIds != null && model.CusTagIds.Any())
            {
                var customerList = new Custom().GetList(model.StoreIds.ToArray(), model.CusTagIds.ToArray());
                if (customerList != null && customerList.Count > 0 && model.RedeemPoints > 0)
                {
                    cus = customerList.Find(o => o.CusCurrentScore > model.RedeemPoints);
                    if (cus == null)
                    {
                        result.Success = false;
                        result.Message = "积分不足";
                        return result;
                    }
                }
                else
                {
                    cus = customerList.FirstOrDefault();
                }
            }
            if (model.cusIds != null && model.cusIds.Any())
            {
                if (cusList != null && cusList.Count > 0 && model.RedeemPoints > 0)
                {
                    cus = cusList.Find(o => o.CusCurrentScore > model.RedeemPoints);
                    if (cus == null)
                    {
                        result.Success = false;
                        result.Message = "积分不足";
                        return result;
                    }
                }
                else
                {
                    cus = cusList.Find(o => model.cusIds.Contains(o.Id));
                }
            }
            if (cus != null && !string.IsNullOrWhiteSpace(cus.Id))
            {
                result.Success = true;
                result.ReturnData = new CustomerInfo();
                result.ReturnData.Id = cus.Id;
                result.ReturnData.CusStoreId = cus.StoreId;
                result.ReturnData.CusName = cus.CusName;
                result.ReturnData.CusPhoneNo = cus.CusPhoneNo;
            }
            else
            {
                result.Success = false;
                result.Message = "不符合条件的客户";
            }
            return result;
        }

        /// <summary>
        /// 获取自动发放卡卷的用户信息
        /// </summary>
        /// <param name="storeIds"></param>
        /// <param name="cusLevelIds"></param>
        /// <param name="cusLabelIds"></param>
        /// <returns></returns>
        public static OperatResult<List<CustomerInfo>> GetAutomaticCusInfo(string mctNum, string[] storeIds, string[] cusLevelIds, string[] cusLabelIds, string[] cusIds)
        {
            OperatResult<List<CustomerInfo>> res = new OperatResult<List<CustomerInfo>>();
            List<CustomerInfo> cusInfo = new List<CustomerInfo>();
            if (cusLevelIds != null && cusLevelIds.Count() > 0)
            {
                var resLevelList = new CustomerLevel().GetList(mctNum);
                if (resLevelList != null && resLevelList.Count > 0)
                {
                    var levelList = resLevelList.FindAll(o => cusLevelIds.Contains(o.Id));
                    var IsOrdinaryMember = false;
                    var ordinaryMember = levelList.Find(o => o.Grade == "普通客户");
                    if (ordinaryMember != null)
                    {
                        IsOrdinaryMember = true;
                    }
                    var resCustomerList = new Data.Custom().GetList(storeIds, cusLevelIds, IsOrdinaryMember);
                    if (resCustomerList != null)
                    {
                        foreach (var item in resCustomerList)
                        {
                            CustomerInfo info = new CustomerInfo()
                            {
                                CusName = item.CusName,
                                CusPhoneNo = item.CusPhoneNo,
                                Id = item.Id,
                                CusStoreId = item.StoreId,
                                CusNo = item.CusNo
                            };
                            cusInfo.Add(info);
                        }
                        res.Success = true;
                        res.Message = "获取成功";
                        res.ReturnData = cusInfo;
                        return res;
                    }
                }
            }
            if (cusLabelIds != null && cusLabelIds.Count() > 0)
            {
                var resCustomerList = new Data.Custom().GetList(storeIds, cusLabelIds);
                if (resCustomerList != null)
                {
                    foreach (var item in resCustomerList)
                    {
                        CustomerInfo info = new CustomerInfo()
                        {
                            CusName = item.CusName,
                            CusPhoneNo = item.CusPhoneNo,
                            Id = item.Id,
                            CusStoreId = item.StoreId,
                            CusNo = item.CusNo
                        };
                        cusInfo.Add(info);
                    }
                    res.Success = true;
                    res.Message = "获取成功";
                    res.ReturnData = cusInfo;
                    return res;
                }
            }
            if (cusIds != null && cusIds.Count() > 0)
            {
                var resCustomerList = new Data.Custom().GetList(mctNum, storeIds, cusIds);
                if (resCustomerList != null)
                {
                    foreach (var item in resCustomerList)
                    {
                        CustomerInfo info = new CustomerInfo()
                        {
                            CusName = item.CusName,
                            CusPhoneNo = item.CusPhoneNo,
                            Id = item.Id,
                            CusStoreId = item.StoreId,
                            CusNo = item.CusNo
                        };
                        cusInfo.Add(info);
                    }
                    res.Success = true;
                    res.Message = "获取成功";
                    res.ReturnData = cusInfo;
                    return res;
                }
            }
            return res;
        }

        /// <summary>
        /// 根据门店列表跟客户手机号获取客户信息
        /// </summary>
        /// <param name="storeIds"></param>
        /// <param name="cusPhoneNo"></param>
        /// <returns></returns>
        public static OperatResult<CustomerInfo> GetCustomerByPhoneNoAndStoreIds(string[] storeIds, string cusPhoneNo)
        {
            OperatResult<CustomerInfo> res = new OperatResult<CustomerInfo>();
            var resModel = new Custom().GetByPhone(cusPhoneNo, storeIds);
            if (resModel != null)
            {
                CustomerInfo model = new CustomerInfo()
                {
                    CusName = resModel.CusName,
                    CusNo = resModel.CusNo,
                    CusPhoneNo = cusPhoneNo,
                    CusStoreId = resModel.StoreId,
                    Id = resModel.Id
                };

                res.Success = true;
                res.Message = "获取成功";
                res.ReturnData = model;
            }
            else
            {
                res.Success = true;
                res.Message = "暂无数据";
                res.ReturnData = null;
            }
            return res;
        }

        /// <summary>
        /// 根据商户号手机号获取列表 积分商城卡券列表调用
        /// </summary>
        /// <param name="storeIds"></param>
        /// <param name="cusPhoneNo"></param>
        /// <returns></returns>
        public static OperatResult<List<CustomerInfo>> GetCustomerByPhoneNoAndMct(string mctNum, string cusPhoneNo)
        {
            OperatResult<List<CustomerInfo>> res = new OperatResult<List<CustomerInfo>>();
            var resList = new Custom().GetListByPhoneAndMct(mctNum, cusPhoneNo);
            if (resList != null && resList.Count > 0)
            {
                List<CustomerInfo> list = new List<CustomerInfo>();
                foreach (var item in resList)
                {
                    CustomerInfo model = new CustomerInfo()
                    {
                        CusName = item.CusName,
                        CusNo = item.CusNo,
                        CusPhoneNo = cusPhoneNo,
                        CusStoreId = item.StoreId,
                        Id = item.Id,
                        CusLevelId = item.CusLevelId,
                        CusCurrentScore = Convert.ToDecimal(item.CusCurrentScore),
                    };
                    list.Add(model);
                }
                res.Success = true;
                res.Message = "获取成功";
                res.ReturnData = list;
            }
            else
            {
                res.Success = true;
                res.Message = "暂无数据";
                res.ReturnData = null;
            }
            return res;
        }

        /// <summary>
        /// 微信放开领券时适用门店没有客户信息时根据商户，客户手机获取一条客户记录插入到适用门店中
        /// </summary>
        /// <param name="storeIds"></param>
        /// <param name="cusPhoneNo"></param>
        /// <returns></returns>
        public static OperatResult<CustomerInfo> CreateCustomerByMctNumAndPhone(string mctNum, string cusPhoneNo, string storeId)
        {
            OperatResult<CustomerInfo> res = new OperatResult<CustomerInfo>();
            var resList = new Custom().GetListByPhoneAndMct(mctNum, cusPhoneNo);
            if (resList != null && resList.Count > 0)
            {
                var chief = StoreAndStaffServiceTransfer.GetChiefByMct(mctNum);
                if (chief == null)
                {
                    res.ReturnData = null;
                    res.Message = "找不到总部信息";
                    res.Success = false;
                    return res;
                }
                decimal? cusInitialIntegral = 0;
                OperatResult<List<IntegralRulesModel>> integralRulesServiceRet = IntegralRulesService.GetList(mctNum);
                if (integralRulesServiceRet != null && integralRulesServiceRet.ReturnData != null && integralRulesServiceRet.ReturnData.Count > 0)
                {
                    if (integralRulesServiceRet.ReturnData.Find(s => s.Status == true && s.IntegralRuleId == WJewel.DataContract.CRM.Common.CommonIntegralRuleModel.AddForRegister) != null)
                    {
                        cusInitialIntegral = integralRulesServiceRet.ReturnData.Find(s => s.Status == true && s.IntegralRuleId == WJewel.DataContract.CRM.Common.CommonIntegralRuleModel.AddForRegister).IntegralMechanism;
                    }
                }
                Cus_CustomerLevel ordinaryMember = null;
                var cusLevelList = new CustomerLevel().GetList(mctNum, chief.Id);
                if (cusLevelList != null)
                {
                    ordinaryMember = cusLevelList.Find(o => o.Grade == "普通客户");
                }
                var customer = resList.First();
                customer.Id = SecureHelper.GetNum();
                customer.CusAccumulatedPoints = (decimal)cusInitialIntegral;
                customer.CusCurrentScore = (decimal)cusInitialIntegral;
                customer.CusFaceId = 0;
                customer.CusInitialIntegral = (decimal)cusInitialIntegral;
                customer.CusIsBindWX = false;
                customer.CusLevelId = ordinaryMember != null ? ordinaryMember.Id : "";
                customer.CusNo = GetCusNo("", mctNum);
                customer.CusRegisterTime = DateTime.Now;
                customer.CusRemark = "";
                customer.CusWechatNo = "";
                customer.IsActivation = false;
                customer.IsDelete = false;
                customer.Status = true;
                customer.StoreId = storeId;

                List<Cus_StaffCustomer> staffList = new List<Cus_StaffCustomer>();
                Cus_StaffCustomer staff = new Cus_StaffCustomer()
                {
                    CustomerId = customer.Id,
                    DistributionId = "",
                    DistributionTime = DateTime.Now,
                    Id = SecureHelper.GetNum(),
                    MctNum = customer.MctNum,
                    StaffId = "",
                    StaffName = "",
                    StoreId = customer.StoreId,
                };
                staffList.Add(staff);

                List<Cus_IntegralRecord> integralList = new List<Cus_IntegralRecord>();
                if (cusInitialIntegral > 0)
                {
                    Cus_IntegralRecord integralRecord = new Cus_IntegralRecord()
                    {
                        Id = SecureHelper.GetNum(),
                        StoreId = customer.StoreId,
                        ERPOrderNo = null,
                        ChangeType = 1,
                        AffectedNumber = (decimal)customer.CusCurrentScore,
                        AffectedMoney = 0,
                        ScoreBalance = (decimal)customer.CusCurrentScore,
                        BusinessStoreId = customer.StoreId,
                        BusinessStaffId = "0",
                        IntegralRulesId = WJewel.DataContract.CRM.Common.CommonIntegralRuleModel.AddForRegister,
                        IntegralType = WJewel.DataContract.CRM.Common.CommonIntegralTypeModel.AddForTask,
                        MainSalerId = null,
                        MctNum = customer.MctNum,
                        Remark = "注册初始化积分",
                        CustomerId = customer.Id,
                        CreatedDate = DateTime.Now,
                    };
                    integralList.Add(integralRecord);
                }
                if (new Custom().Create(new List<Cus_Customer>() { customer }, null, integralList, staffList, null))
                {
                    CustomerInfo info = new CustomerInfo()
                    {
                        Id = customer.Id,
                        CusCurrentScore = 0,
                        CusLevelId = customer.CusLevelId,
                        CusName = customer.CusName,
                        CusNo = customer.CusNo,
                        CusPhoneNo = customer.CusPhoneNo,
                        CusStoreId = customer.StoreId,
                    };

                    res.Success = true;
                    res.Message = "获取成功";
                    res.ReturnData = info;
                }
                else
                {
                    res.Success = true;
                    res.Message = "暂无数据";
                    res.ReturnData = null;
                }
            }
            else
            {
                res.Success = true;
                res.Message = "暂无数据";
                res.ReturnData = null;
            }
            return res;
        }

        /// <summary>
        /// 根据客户编号获取客户标签编号  卡券积分商城列表
        /// </summary>
        /// <param name="cusIds"></param>
        /// <returns></returns>
        public static OperatResult<List<TagsShopModel>> GetCustomerTags(string[] cusIds)
        {
            OperatResult<List<TagsShopModel>> res = new OperatResult<List<TagsShopModel>>();
            List<TagsShopModel> list = new List<TagsShopModel>();
            var resTags = new Custom().GetCustomerTags(cusIds);
            if (resTags != null && resTags.Count > 0)
            {
                foreach (var item in resTags)
                {
                    list.Add(new TagsShopModel() { LabelId = item.LabelId, StoreId = item.StoreId });
                }
            }
            res.ReturnData = list;
            res.Success = true;
            res.Message = "获取成功";
            return res;
        }
        
        /// <summary>
        /// 根据客户累计积分
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cusAccumulatedPoints"></param>
        /// <returns></returns>
        public static OperatResult<CustomerLevelModel> GetCustomerLevel(decimal cusAccumulatedPoints, string mctNum)
        {
            OperatResult<CustomerLevelModel> res = new OperatResult<CustomerLevelModel>();
            var chief = StoreAndStaffServiceTransfer.GetChiefByMct(mctNum);
            if (chief == null)
            {
                res.ReturnData = null;
                res.Message = "找不到总部信息";
                res.Success = false;
                return res;
            }
            var cusLevelList = new CustomerLevel().GetList(mctNum, chief.Id);
            if (cusLevelList != null && cusLevelList.Count > 0)
            {
                var levelList = cusLevelList.FindAll(o => o.AchieveConditions <= cusAccumulatedPoints);
                if (levelList != null && levelList.Count > 0)
                {
                    var level = levelList.OrderByDescending(o => o.AchieveConditions).FirstOrDefault();
                    if (level != null && level.Grade == "游客")
                    {
                        level = cusLevelList.Find(o => o.Grade == "普通客户");
                    }
                    if (level != null)
                    {
                        CustomerLevelModel model = new CustomerLevelModel()
                        {
                            Id = level.Id,
                            AchieveConditions = level.AchieveConditions,
                            BirthdayConsumption = level.BirthdayConsumption,
                            Grade = level.Grade,
                            IsSysrtemPreset = level.IsSysrtemPreset,
                            OrdinaryConsumption = level.OrdinaryConsumption,
                            OtherEquity = level.OtherEquity,
                            ProductOffer = level.ProductOffer,
                            UpgradeReward = level.UpgradeReward,
                            Weights = level.Weights,
                        };
                        res.Success = true;
                        res.Message = "获取成功";
                        res.ReturnData = model;
                        return res;
                    }
                    else
                    {
                        res.Success = false;
                        res.Message = "暂无等级信息";
                        res.ReturnData = null;
                        return res;
                    }
                }
                else
                {
                    res.Success = false;
                    res.Message = "暂无等级信息";
                    res.ReturnData = null;
                    return res;
                }
            }
            else
            {
                res.Success = false;
                res.Message = "暂无等级信息";
                res.ReturnData = null;
                return res;
            }
        }

        /// <summary>
        /// 编辑客户微信卡包积分和等级等信息 
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public static OperatResult<bool> ModifyWxCardInfo(UserData ud, string customerId, int integral)
        {
            OperatResult<bool> result = new OperatResult<bool>() { Success = false, ReturnData = false };
            try
            {
                var customerInfo = new Custom().GetById(customerId);
                if (customerInfo == null)
                {
                    result.Message = "客户不存在";
                    return result;
                }
                //var cardReceive = WXTPVIPCardServiceTransfer.GetCardReceive(customerInfo.CusPhoneNo, customerInfo.StoreId, ud.TokenStr);
                var cardReceive = WXTPVIPCardServiceTransfer.GetCardReceive(customerInfo.CusPhoneNo, customerInfo.MctNum, customerInfo.StoreId, ud.TokenStr);
                if (cardReceive == null)
                {
                    result.Message = "客户没有绑定微信公众号";
                    return result;
                }
                var maxCus = new Custom().GetMaxIntegralCus(customerInfo.CusPhoneNo, customerInfo.MctNum); //如果是微信客户登录后使用ud.MctNum,可能为空
                if (maxCus == null)
                {
                    result.Message = "客户不存在";
                    return result;
                }
                var cusLevel = new CustomerLevel().GetById(maxCus.CusLevelId);
                if (cusLevel == null)
                {
                    cusLevel = new Cus_CustomerLevel();
                    cusLevel.Grade = "普通会员";
                }
                var updateCardInfoRet = WXTPVIPCardServiceTransfer.UpdateCardInfo(cardReceive.CardId, cardReceive.CardCode, ud.TokenStr, maxCus.CusCurrentScore == null ? 0 : maxCus.CusCurrentScore.Value.Toint(), integral, cusLevel.Grade, null);
                if (updateCardInfoRet != null && updateCardInfoRet.Data)
                {
                    result.Success = true;
                    result.ReturnData = true;
                }
                else
                {
                    result.Message = $"客户微信卡包信息编辑失败：{(updateCardInfoRet != null ? updateCardInfoRet.Message : "updateCardInfoRet为空")}";
                    return result;
                }
            }
            catch (Exception ex)
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// 编辑客户微信卡包积分和等级等信息 （ERP导入调用）
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public static OperatResult<bool> ModifyWxCardInfo(string customerId, int integral)
        {
            OperatResult<bool> result = new OperatResult<bool>() { Success = false, ReturnData = false };
            try
            {
                var customerInfo = new Custom().GetById(customerId);
                if (customerInfo == null)
                {
                    result.Message = "客户不存在";
                    return result;
                }
                var cardReceive = WXTPVIPCardServiceTransfer.GetCardReceiveByErp(customerInfo.CusPhoneNo, customerInfo.MctNum, customerInfo.StoreId);
                if (cardReceive == null)
                {
                    result.Message = "客户没有绑定微信公众号";
                    return result;
                }
                var maxCus = new Custom().GetMaxIntegralCus(customerInfo.CusPhoneNo, customerInfo.MctNum); //如果是微信客户登录后使用ud.MctNum,可能为空
                if (maxCus == null)
                {
                    result.Message = "客户不存在";
                    return result;
                }
                var cusLevel = new CustomerLevel().GetById(maxCus.CusLevelId);
                if (cusLevel == null)
                {
                    cusLevel = new Cus_CustomerLevel();
                    cusLevel.Grade = "普通会员";
                }
                var updateCardInfoRet = WXTPVIPCardServiceTransfer.UpdateCardInfosByErp(cardReceive.CardId, cardReceive.CardCode, maxCus.CusCurrentScore == null ? 0 : maxCus.CusCurrentScore.Value.Toint(), integral, cusLevel.Grade, null);
                if (updateCardInfoRet != null && updateCardInfoRet.Data)
                {
                    result.Success = true;
                    result.ReturnData = true;
                }
                else
                {
                    result.Message = $"客户微信卡包信息编辑失败：{(updateCardInfoRet != null ? updateCardInfoRet.Message : "updateCardInfoRet为空")}";
                    return result;
                }
            }
            catch (Exception ex)
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// 自动发送客户生日祝福语
        /// </summary>
        public static void SendBirthdayWish()
        {
            var cusList = new Custom().GetHashBirthdayCustomList(DateTime.Now.ToString("yy-MM-dd").ToDateTime());
            if (cusList != null && cusList.Any())
            {
                var mobileList = (from a in cusList group a by new { a.CusPhoneNo, a.MctNum } into g select new { g.Key.CusPhoneNo, g.Key.MctNum }).ToList();
                mobileList.ForEach(s =>
                {
                    var user = UserCenterServiceTransfer.GetUserByMobile(null, s.CusPhoneNo);
                    if (user != null)
                    {
                        var msg = new SendTextMessageModel()
                        {
                            UserId = user.Id,
                            PlatId = CommonPlatModel.Customer,
                            MctNum = s.MctNum,
                            Content = $"尊敬的会员，今天是您的生日~"
                        };
                        WXTPMessageServiceTransfer.SendTextMessageNoLogin(null, msg);
                    }
                });
            }
        }


        /// <summary>
        /// 生成消息数据
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="customer"></param>
        /// <param name="msgType"></param>
        /// <param name="exchangeScore"></param>
        public static void GenerateTaskMessage(Cus_Customer customer, MessageTemplateEnum msgType, string openId)
        {
            var tempResult = MessageServiceTransfer.GetTempTypeForBusinessNoToken(customer.MctNum, customer.StoreId, (int)msgType); //获取模版
            if (tempResult != null && tempResult.ReturnData != null && tempResult.ReturnData.Any())
            {
                var placeholderList = PlaceholderServiceTransfer.GetPlaceholderListNoToken();
                var tempList = tempResult.ReturnData;
                string path = HttpRuntime.AppDomainAppPath.ToString();
                var asm = ObjectHelper.GetAssembly(path + "\\bin");
                if (asm != null)
                {
                    var strList = new Dictionary<string, string>();
                    Regex reg = new Regex(@"(?<=\【)[^】]*(?=\】)");
                    var follower = new StaffCustomer().GetInfo(customer.StoreId, customer.Id);
                    var store = StoreAndStaffServiceTransfer.GetStoreByIdNoToken(customer.StoreId);
                    tempList.ForEach(temp =>
                    {
                        if (!string.IsNullOrWhiteSpace(temp.Opening))  //获取开场白的占位符id
                        {
                            var openMctchs = reg.Matches(temp.Opening);
                            if (openMctchs != null && openMctchs.Count > 0)
                            {
                                foreach (var openMctch in openMctchs)
                                {
                                    if (placeholderList != null && placeholderList.Any())
                                    {
                                        var p = placeholderList.Find(sp => sp.Id == openMctch.ToString());
                                        if (p != null && !strList.ContainsKey(openMctch.ToString()))  //key值不存在
                                        {
                                            strList.Add(openMctch.ToString(), p.FiledName);
                                        }
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(temp.Lists))//获取列表的占位符id
                        {
                            var listMctchs = reg.Matches(temp.Lists);
                            if (listMctchs != null && listMctchs.Count > 0)
                            {
                                foreach (var listMctch in listMctchs)
                                {
                                    if (placeholderList != null && placeholderList.Any())
                                    {
                                        var p = placeholderList.Find(sp => sp.Id == listMctch.ToString());
                                        if (p != null && !strList.ContainsKey(listMctch.ToString()))  //key值不存在
                                        {
                                            strList.Add(listMctch.ToString(), p.FiledName);
                                        }
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(temp.Conclusion))//获取结束语的占位符id
                        {
                            var conclusionMctchs = reg.Matches(temp.Conclusion);
                            if (conclusionMctchs != null && conclusionMctchs.Count > 0)
                            {
                                foreach (var conclusionMctch in conclusionMctchs)
                                {
                                    if (placeholderList != null && placeholderList.Any())
                                    {
                                        var p = placeholderList.Find(sp => sp.Id == conclusionMctch.ToString());
                                        if (p != null && !strList.ContainsKey(conclusionMctch.ToString()))  //key值不存在
                                        {
                                            strList.Add(conclusionMctch.ToString(), p.FiledName);
                                        }
                                    }
                                }
                            }
                        }
                        if (strList.Any())
                        {
                            var levelName = "";
                            if (string.IsNullOrWhiteSpace(customer.CusLevelId))
                            {
                                levelName = "普通会员";
                            }
                            else
                            {
                                var level = new CustomerLevel().GetById(customer.CusLevelId);
                                if (level == null)
                                {
                                    levelName = "普通会员";
                                }
                                else
                                {
                                    levelName = level.Grade;
                                }
                            }
                            var msgModel = new CustomerRegisterForMsgModel()
                            {
                                CusName = customer.CusName,
                                CusCurrentScore = customer.CusCurrentScore.Value,
                                CusPhoneNo = customer.CusPhoneNo,
                                CusLevelName = levelName,
                                StoreName = store?.StoreName,
                                OperateTime = DateTime.Now
                            };
                            CreateTaskMessage(temp, asm, strList, msgModel, customer, msgType, openId);
                        }
                    });
                }
            }
        }

        public static void CreateTaskMessage(MessageTempModelForBusiness temp, Assembly asm, Dictionary<string, string> strList, CustomerRegisterForMsgModel msgModel, Cus_Customer customer, MessageTemplateEnum msgType, string openId)
        {
            var dicList = ObjectHelper.GetFieldNameAndValue<CustomerRegisterForMsgModel>(asm, msgModel, strList); //获取占位符对应的数据
            if (dicList != null && dicList.Count > 0)
            {
                try
                {
                    foreach (var dic in dicList)
                    {
                        if (dic.Value != null)
                        {
                            if (!string.IsNullOrWhiteSpace(temp.Opening))
                            {
                                temp.Opening = temp.Opening.Replace(dic.Key, dic.Value.ToString()); //替换占位符的值
                            }
                            if (!string.IsNullOrWhiteSpace(temp.Lists))
                            {
                                temp.Lists = temp.Lists.Replace(dic.Key, dic.Value.ToString()); //替换占位符的值
                            }
                            if (!string.IsNullOrWhiteSpace(temp.Conclusion))
                            {
                                temp.Conclusion = temp.Conclusion.Replace(dic.Key, dic.Value.ToString()); //替换占位符的值
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(temp.Opening))
                            {
                                temp.Opening = temp.Opening.Replace(dic.Key, ""); //替换占位符的值
                            }
                            if (!string.IsNullOrWhiteSpace(temp.Lists))
                            {
                                temp.Lists = temp.Lists.Replace(dic.Key, ""); //替换占位符的值
                            }
                            if (!string.IsNullOrWhiteSpace(temp.Conclusion))
                            {
                                temp.Conclusion = temp.Conclusion.Replace(dic.Key, ""); //替换占位符的值
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                if (!string.IsNullOrWhiteSpace(temp.Opening))
                {
                    temp.Opening = temp.Opening.Replace("【", "").Replace("】", "");//替换【】
                }
                if (!string.IsNullOrWhiteSpace(temp.Lists))
                {
                    temp.Lists = temp.Lists.Replace("【", "").Replace("】", "");//替换【】
                }
                if (!string.IsNullOrWhiteSpace(temp.Conclusion))
                {
                    temp.Conclusion = temp.Conclusion.Replace("【", "").Replace("】", ""); //替换【】
                }
                if (!string.IsNullOrWhiteSpace(openId)) //如果有用户数据 并且 openid不为空
                {
                    var taskMsg = new CreateTaskMessageModel();
                    taskMsg.MessageType = (int)msgType;
                    taskMsg.MessageName = temp.MessageName;
                    taskMsg.SendStatus = 0;
                    taskMsg.SendTime = msgModel.OperateTime;
                    DateTime sendTime = msgModel.OperateTime;
                    if (temp.PublishTimeId == "0")
                    {
                        if (string.IsNullOrWhiteSpace(temp.SendTime))
                        {
                            taskMsg.SendTime = temp.SendTime.ToDateTime();
                        }
                        else
                        {
                            taskMsg.SendTime = sendTime;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (temp.SendMoment == 0 && temp.SendDay.Value > 0)
                            {
                                sendTime = sendTime.AddDays(temp.SendDay.Value * -1);
                                taskMsg.SendTime = (sendTime.Date.ToString() + " " + temp.SendTime).ToDateTime();
                            }
                            if (temp.SendMoment == 1 && temp.SendDay.Value > 0)
                            {
                                sendTime = sendTime.AddDays(temp.SendDay.Value);
                                taskMsg.SendTime = (sendTime.Date.ToString() + " " + temp.SendTime).ToDateTime();
                            }
                            if (temp.SendMoment == 2)
                            {
                                taskMsg.SendTime = msgModel.OperateTime;
                            }
                        }
                        catch (Exception)
                        {
                            taskMsg.SendTime = msgModel.OperateTime;
                        }
                    }
                    string authUrl = string.Empty;
                    if (!string.IsNullOrWhiteSpace(temp.LinkUrl))
                    {
                        authUrl = WXTPPublicNumberServiceTransfer.GetWXOAuthUrlNoToken(HttpUtility.UrlEncode(temp.LinkUrl), customer.MctNum);
                    }

                    if (temp.IsSupportCompanyWX)
                    {
                        taskMsg.PublishWay = 1;
                        taskMsg.CusName = customer.CusName;
                        taskMsg.CusNo = customer.CusNo;
                        taskMsg.CusPhoneNo = customer.CusPhoneNo;
                        taskMsg.SendTo = openId;
                        taskMsg.MctNum = customer.MctNum;
                        taskMsg.StoreId = customer.StoreId;
                        taskMsg.SendContent = temp.Opening + "\r\n" + temp.Lists + "\r\n" + temp.Conclusion;
                        if (!string.IsNullOrWhiteSpace(authUrl))
                        {
                            taskMsg.SendContent += "\r\n" + $"<a href='{authUrl}'>{temp.LinkName}</a>";
                        }
                        taskMsg.PublishId = temp.Id;
                        MessageServiceTransfer.CreateMesageNoToken(taskMsg);
                    }
                    if (temp.IsSupportWX)
                    {
                        taskMsg.PublishWay = 2;
                        taskMsg.CusName = customer.CusName;
                        taskMsg.CusNo = customer.CusNo;
                        taskMsg.CusPhoneNo = customer.CusPhoneNo;
                        taskMsg.SendTo = openId;
                        taskMsg.MctNum = customer.MctNum;
                        taskMsg.StoreId = customer.StoreId;
                        taskMsg.SendContent = temp.Opening + "\r\n" + temp.Lists + "\r\n" + temp.Conclusion;
                        if (!string.IsNullOrWhiteSpace(authUrl))
                        {
                            taskMsg.SendContent += "\r\n" + $"<a href='{authUrl}'>{temp.LinkName}</a>";
                        }
                        taskMsg.PublishId = temp.Id;
                        MessageServiceTransfer.CreateMesageNoToken(taskMsg);
                    }
                    if (temp.IsSupportStoreWX)
                    {
                        taskMsg.PublishWay = 3;
                        taskMsg.CusName = customer.CusName;
                        taskMsg.CusNo = customer.CusNo;
                        taskMsg.CusPhoneNo = customer.CusPhoneNo;
                        taskMsg.SendTo = openId;
                        taskMsg.MctNum = customer.MctNum;
                        taskMsg.StoreId = customer.StoreId;
                        taskMsg.SendContent = temp.Opening + "\r\n" + temp.Lists + "\r\n" + temp.Conclusion;
                        if (!string.IsNullOrWhiteSpace(authUrl))
                        {
                            taskMsg.SendContent += "\r\n" + $"<a href='{authUrl}'>{temp.LinkName}</a>";
                        }
                        taskMsg.PublishId = temp.Id;
                        MessageServiceTransfer.CreateMesageNoToken(taskMsg);
                    }
                }
            }
        }


        /// <summary>
        /// 获取会员报表
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="storeIds"></param>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static OperatResult<CustomerAnalysisModel> CustomerStatistics(UserData ud, string storeIds, string dateStart, string dateEnd)
        {
            OperatResult<CustomerAnalysisModel> operatResult = new OperatResult<CustomerAnalysisModel>();
            try
            {
                var custom = new Custom();
                var gradeList = new CustomerLevel().GetList(ud.MctNum);
                var dataAll = custom.StatisticsList(ud.MctNum, storeIds);//取全部客户统计
                var dataOne = custom.StatisticsList(ud.MctNum, storeIds, dateStart, dateEnd);//取查询日期统计
                var dataTwo = custom.StatisticsList(ud.MctNum, storeIds, Convert.ToDateTime(dateStart).AddYears(-1).ToString("yyyy-MM-dd"), Convert.ToDateTime(dateEnd).AddYears(-1).ToString("yyyy-MM-dd"));//取同比统计
                var customNum = WisdomServiceTransfer.GetFaceCustomerCount(ud.TokenStr, storeIds, dateStart, dateEnd);//查询日期内会员进店数
                var faceonedata = WisdomServiceTransfer.GetFaceTouristCount(ud.TokenStr, storeIds, dateStart, dateEnd);//查询日期内游客进店数

                CustomerAnalysisModel model = new CustomerAnalysisModel();
                model.TotalNumber = 0;
                model.TotalCustomerNumber = 0;
                model.customerGrades = new List<CustomerGradeModel>();

                foreach (var gradeitem in gradeList)
                {
                    var grade = new CustomerGradeModel()
                    {
                        Grade = gradeitem.Grade,
                        TotalNumber = 0,
                    };
                    if (dataAll.ContainsKey(gradeitem.Grade))
                    {
                        var item = dataAll[gradeitem.Grade];
                        grade.TotalNumber = item;
                        if (dataOne.ContainsKey(gradeitem.Grade))
                            grade.AddNumber = dataOne[gradeitem.Grade];
                        if (dataTwo.ContainsKey(gradeitem.Grade))
                            grade.YonYNumber = dataOne[gradeitem.Grade];

                        model.TotalNumber += item;//会员数加入到客户总数
                        model.TotalCustomerNumber += item;//会员数加入会员总数
                    }
                    if (gradeitem.Grade == faceonedata.Grade)
                    {
                        grade.TotalNumber += faceonedata.TotalNumber;
                        grade.AddNumber += faceonedata.AddNumber;
                        grade.YonYNumber += faceonedata.YonYNumber;

                        model.TotalNumber += faceonedata.TotalNumber;//把游客数加入到客户总数
                    }
                    model.customerGrades.Add(grade);
                }

                model.customerGrades.ForEach(t =>
                {
                    if (model.TotalNumber > 0)
                        t.Rate = System.Decimal.Round(t.TotalNumber * 100.00m / model.TotalNumber, 2);
                    if (t.YonYNumber > 0)
                        t.YearonYearRate = System.Decimal.Round((t.TotalNumber - t.YonYNumber) * 100.00m / t.YonYNumber, 2);
                });

                var purchaseCount = new IntegralRecord().PurchaseStatisticsCount(ud.MctNum, storeIds, dateStart, dateEnd);
                if (model.TotalNumber > 0)
                {
                    model.PurchaseRate = System.Decimal.Round(purchaseCount * 100.00m / model.TotalNumber, 2);
                    model.StoreRate = System.Decimal.Round(customNum * 100.00m / model.TotalNumber, 2);
                }
                operatResult.ReturnData = model;

                operatResult.Message = $"会员统计";
                operatResult.Success = true;

            }
            catch (Exception ex)
            {
                operatResult.Message = $"会员统计失败，{ex.Message}";
                operatResult.Success = false;
            }

            return operatResult;
        }

        /// <summary>
        /// 僵尸粉分析
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="storeIds"></param>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static OperatResult<List<ZombiesStatisticsModel>> ZombieCustomerStatistics(UserData ud, int mode, string regStart, string regEnd, string storeIds)
        {
            OperatResult<List<ZombiesStatisticsModel>> operatResult = new OperatResult<List<ZombiesStatisticsModel>>();
            try
            {
                var key = $"zmc{mode}{regStart}{regEnd}{WJewel.Basic.SecureHelper.MD5(storeIds)}";
                var model = new List<ZombiesStatisticsModel>();
                var dateStart = DateTime.Now;
                var dateEnd = DateTime.Now.AddDays(-1);
                switch (mode)
                {
                    case 1:
                        dateStart = DateTime.Now.AddMonths(-3);
                        break;
                    case 2:
                        dateStart = DateTime.Now.AddMonths(-6);
                        break;
                    case 3:
                        dateStart = DateTime.Now.AddMonths(-12);
                        break;
                    default:
                        break;
                }

                var custom = new Custom();
                var data = new List<VZombieCustom>();
                var zombiesCusAllList = new List<VZombieCustom>();
                var faceCustom = new List<string>();
                var redis = new RedisHelper();
                if (redis != null)
                {
                    data = redis.HashGet<List<VZombieCustom>>(RedisPrimaryKey.ACTIVECUSTOMDETAIL, key);
                    zombiesCusAllList = redis.HashGet<List<VZombieCustom>>(RedisPrimaryKey.ZOMBIESCUSTOMDETAIL, key);
                    faceCustom = redis.HashGet<List<string>>(RedisPrimaryKey.ZOMBIESCUSTOMFACEDETAIL, key);
                }
                if (data == null || data.Count <= 0)
                {
                    data = custom.ZombieCustomerList(ud.MctNum, storeIds, regStart, regEnd, dateStart.ToString("yyyy-MM-dd"), dateEnd.ToString("yyyy-MM-dd"));
                    faceCustom = WisdomServiceTransfer.GetFaceCustomerList(ud.TokenStr, storeIds, dateStart.ToString("yyyy-MM-dd"), dateEnd.ToString("yyyy-MM-dd"));
                    zombiesCusAllList = data.Where(t => string.IsNullOrEmpty(t.ActiveCustomerId)).ToList();
                    foreach (var item in faceCustom)
                    {
                        var temp = zombiesCusAllList.Where(t => t.CustomerId == item).FirstOrDefault();
                        if (temp != null)
                            zombiesCusAllList.Remove(temp);
                    }
                    if (redis != null)
                    {
                        redis.HashSet<List<VZombieCustom>>(RedisPrimaryKey.ACTIVECUSTOMDETAIL, key, data, DateTime.Now.AddHours(10).TimeOfDay);
                        redis.HashSet<List<VZombieCustom>>(RedisPrimaryKey.ZOMBIESCUSTOMDETAIL, key, zombiesCusAllList, DateTime.Now.AddHours(10).TimeOfDay);
                        redis.HashSet<List<string>>(RedisPrimaryKey.ZOMBIESCUSTOMFACEDETAIL, key, faceCustom, DateTime.Now.AddHours(10).TimeOfDay);
                    }
                }

                var gradeList = new CustomerLevel().GetList(ud.MctNum);
                foreach (var item in gradeList)
                {
                    var zombies = new ZombiesStatisticsModel();
                    zombies.CustomerLevelId = item.Id;
                    zombies.CustomerLevelName = item.Grade;
                    var customers = data.Where(t => t.GradeId == item.Id);
                    if (customers.Count() > 0)
                    {
                        var zombiesCustomers = data.Where(t => t.GradeId == item.Id && string.IsNullOrEmpty(t.ActiveCustomerId));
                        var linq = zombiesCustomers.Join(faceCustom, t => t.CustomerId, f => f, (t, f) => new { t.CustomerId, f });

                        zombies.CustomerCount = customers.Count();
                        zombies.ZombiesCount = zombiesCustomers.Count() - linq.Count();
                        zombies.Percent = zombies.ZombiesCount * 100 / zombies.CustomerCount;
                    }
                    model.Add(zombies);
                }

                operatResult.ReturnData = model;

                operatResult.Message = $"僵尸粉分析";
                operatResult.Success = true;

            }
            catch (Exception ex)
            {
                operatResult.Message = $"僵尸粉分析失败，{ex.Message}";
                operatResult.Success = false;
            }

            return operatResult;
        }
        public static OperatResult<PageList<CustomerListModel>> ZombiesCustomDetails(UserData ud, string gradeId, int mode, string regStart, string regEnd, string storeIds, int pageIndex = 1, int pageSize = 15)
        {
            OperatResult<PageList<CustomerListModel>> operatResult = new OperatResult<PageList<CustomerListModel>>();
            try
            {
                var key = $"zmc{mode}{regStart}{regEnd}{WJewel.Basic.SecureHelper.MD5(storeIds)}";
                var zomkey = $"zmc{gradeId}{mode}{regStart}{regEnd}{WJewel.Basic.SecureHelper.MD5(storeIds)}";
                var model = new PageList<CustomerListModel>();
                var custom = new Custom();
                var data = new List<VZombieCustom>();
                var zombiesCusAllList = new List<VZombieCustom>();
                var zombiesCusList = new List<VZombieCustom>();
                var faceCustom = new List<string>();
                var redis = new RedisHelper();
                if (redis != null)
                {
                    data = redis.HashGet<List<VZombieCustom>>(RedisPrimaryKey.ACTIVECUSTOMDETAIL, key);
                    zombiesCusAllList = redis.HashGet<List<VZombieCustom>>(RedisPrimaryKey.ZOMBIESCUSTOMDETAIL, key);

                    faceCustom = redis.HashGet<List<string>>(RedisPrimaryKey.ZOMBIESCUSTOMFACEDETAIL, key);
                }
                if (data == null || data.Count <= 0)
                {

                    var dateStart = DateTime.Now;
                    var dateEnd = DateTime.Now.AddDays(-1);
                    switch (mode)
                    {
                        case 1:
                            dateStart = DateTime.Now.AddMonths(-3);
                            break;
                        case 2:
                            dateStart = DateTime.Now.AddMonths(-6);
                            break;
                        case 3:
                            dateStart = DateTime.Now.AddMonths(-12);
                            break;
                        default:
                            break;
                    }

                    data = custom.ZombieCustomerList(ud.MctNum, storeIds, regStart, regEnd, dateStart.ToString("yyyy-MM-dd"), dateEnd.ToString("yyyy-MM-dd"));
                    faceCustom = WisdomServiceTransfer.GetFaceCustomerList(ud.TokenStr, storeIds, dateStart.ToString("yyyy-MM-dd"), dateEnd.ToString("yyyy-MM-dd"));
                    zombiesCusAllList = data.Where(t => string.IsNullOrEmpty(t.ActiveCustomerId)).ToList();
                    foreach (var item in faceCustom)
                    {
                        var temp = zombiesCusAllList.Where(t => t.CustomerId == item).FirstOrDefault();
                        if (temp != null)
                            zombiesCusAllList.Remove(temp);
                    }
                    if (redis != null)
                    {
                        redis.HashSet<List<VZombieCustom>>(RedisPrimaryKey.ACTIVECUSTOMDETAIL, key, data, DateTime.Now.AddHours(10).TimeOfDay);
                        redis.HashSet<List<VZombieCustom>>(RedisPrimaryKey.ZOMBIESCUSTOMDETAIL, key, zombiesCusAllList, DateTime.Now.AddHours(10).TimeOfDay);
                        redis.HashSet<List<string>>(RedisPrimaryKey.ZOMBIESCUSTOMFACEDETAIL, key, faceCustom, DateTime.Now.AddHours(10).TimeOfDay);
                    }
                }
                else
                {
                    if (zombiesCusList == null || zombiesCusList.Count <= 0)
                    {
                        zombiesCusList = zombiesCusAllList.Where(t => t.GradeId == gradeId).ToList();
                    }
                }
                if (zombiesCusAllList == null || zombiesCusAllList.Count <= 0)
                    zombiesCusList = zombiesCusAllList.Where(t => t.GradeId == gradeId).ToList();

                if (zombiesCusAllList == null || zombiesCusAllList.Count <= 0)
                {
                    var templist = zombiesCusList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    var totalCount = zombiesCusList.Count;
                    var ids = templist.Select(t => t.CustomerId).ToArray();
                    var ret = custom.GetListByCusId(ud.MctNum, storeIds, ids);
                    if (ret != null && ret.Count > 0)
                    {
                        var levelPageList = CustomerLevelService.GetList(ud, "", 1, 10000);
                        List<CustomerLevelModel> levelList = null;
                        if (levelPageList.Success && levelPageList.ReturnData != null && levelPageList.ReturnData.DataList != null)
                        {
                            levelList = levelPageList.ReturnData.DataList;
                        }
                        var staffList = new StaffCustomer().GetList(ud.StoreId);
                        List<CustomerListModel> list = new List<CustomerListModel>();

                        foreach (var item in ret)
                        {
                            Cus_StaffCustomer staffModel = null;
                            if (staffList != null && staffList.Count > 0)
                            {
                                staffModel = staffList.Find(o => o.CustomerId == item.Id);
                            }
                            CustomerLevelModel tempLevel = null;
                            if (string.IsNullOrWhiteSpace(item.CusLevelId))
                            {
                                tempLevel = levelList.Find(o => o.Grade == "普通客户");
                            }
                            else
                            {
                                tempLevel = levelList.Find(o => o.Id == item.CusLevelId);
                                if (tempLevel == null)
                                    tempLevel = levelList.Find(o => o.Grade == "普通客户");
                            }
                            if (tempLevel == null)
                                tempLevel = new CustomerLevelModel();
                            CustomerListModel clitem = new CustomerListModel()
                            {
                                CusAccumulatedPoints = item.CusAccumulatedPoints,
                                CusBirthday = item.CusBirthday,
                                CusCurrentScore = item.CusCurrentScore,
                                CusFollowPersonName = staffModel != null ? staffModel.StaffName : "",
                                CusLevelId = tempLevel != null ? tempLevel.Id : "",
                                CusLogo = item.CusLogo,
                                CusName = item.CusName,
                                CusNo = item.CusNo,
                                CusPhoneNo = item.CusPhoneNo,
                                CusRemark = item.CusRemark,
                                CusSex = item.CusSex ? "男" : "女",
                                CusWechatNo = item.CusWechatNo,
                                IsActivation = item.IsActivation,
                                Id = item.Id,
                                Status = item.Status,
                                LevelGrade = tempLevel != null ? tempLevel.Grade : "",
                                BirthdayConsumption = tempLevel != null ? tempLevel.BirthdayConsumption : 0,
                                OrdinaryConsumption = tempLevel != null ? tempLevel.OrdinaryConsumption : 0,
                                OtherEquity = tempLevel != null ? tempLevel.OtherEquity : "",
                                ProductOffer = tempLevel != null ? tempLevel.ProductOffer : "",
                                CusInitialIntegral = item.CusInitialIntegral,
                                IsShare = item.StoreId != ud.StoreId ? true : false,
                            };
                            list.Add(clitem);
                        }

                        model.Page = pageIndex;
                        model.PageSize = pageSize;
                        model.TotalCount = totalCount;
                        model.DataList = list;

                        operatResult.ReturnData = model;

                        operatResult.Message = $"僵尸粉分析";
                        operatResult.Success = true;
                    }
                    else
                    {
                        operatResult.Success = true;
                        operatResult.Message = "暂无数据";
                        operatResult.ReturnData = null;
                    }
                }
                else
                {
                    operatResult.Success = true;
                    operatResult.Message = "暂无数据";
                    operatResult.ReturnData = null;
                }

            }
            catch (Exception ex)
            {
                operatResult.Message = $"僵尸粉分析失败，{ex.Message}";
                operatResult.Success = false;
            }

            return operatResult;
        }

        /// <summary>
        /// 发消息用列表
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="paramsModel"></param>
        /// <returns></returns>
        public static OperatResult<List<SearchForMessageListModel>> SearchForMessage(SearchForMessageParamesModel paramsModel)
        {
            OperatResult<List<SearchForMessageListModel>> result = new OperatResult<List<SearchForMessageListModel>>() { Success = false, ReturnData = null };
            try
            {
                string mctNum = "";
                List<string> storeIds = new List<string>();
                if (paramsModel.MctNum == PlatMctNum)//平台获取所有客户
                    mctNum = "";
                else
                {
                    if (!string.IsNullOrWhiteSpace(paramsModel.StoreId))
                    {
                        var storeInfo = StoreAndStaffServiceTransfer.GetStoreByIdNoToken(paramsModel.StoreId);

                        storeIds.Add(paramsModel.StoreId);
                        if (storeInfo.IsChief)
                        {
                            var storeList = StoreAndStaffServiceTransfer.GetStoreListNoToken(paramsModel.StoreId); //获取门店列表（总部获取所有，门店获取所属门店组内所有）
                            if (storeList != null && storeList.Count > 0)
                            {
                                storeIds = storeList.Select(s => s.Id).ToList();
                            }
                        }
                    }

                }
                var list = new Custom().SearchForMessage(mctNum, storeIds.ToArray(), paramsModel.LabelIds, paramsModel.LabelIds, paramsModel.CustomerIds, paramsModel.SaleDateStart, paramsModel.SaleDateEnd);
                if (list != null || list.Count > 0)
                {
                    result.ReturnData = new List<SearchForMessageListModel>();
                    var userList = CrmUserCenterServiceTransfer.GetUserOpenIdAndMobile(mctNum);
                    var levelList = new CustomerLevel().GetList(mctNum);
                    var tagList = new CRM.Customer.Data.CustomerTags().GetList(mctNum);
                    var labelList = new CustomerLabel().GetList(mctNum);
                    foreach (var t in list)
                    {
                        var model = new SearchForMessageListModel()
                        {
                            Id = t.Id,
                            CusCurrentScore = t.CusCurrentScore,
                            CusSex = t.CusSex,
                            CusLevelId = t.CusLevelId,
                            CusName = t.CusName,
                            CusNo = t.CusNo,
                            CusPhoneNo = t.CusPhoneNo,
                            MctNum = t.MctNum,
                            StoreId = t.StoreId
                        };
                        if (levelList != null && levelList.Count > 0)
                        {
                            var level = levelList.Find(s => s.Id == t.CusLevelId);
                            if (level != null)
                                model.CusLevelName = level.Grade;
                        }
                        if (tagList != null && tagList.Count > 0)
                        {
                            var cusTagList = tagList.FindAll(s => s.CusId == t.Id);
                            if (cusTagList != null && cusTagList.Count < 1)
                            {
                                model.LabelList = new List<WJewel.DataContract.CRM.Customer.CustomerTags>();
                                foreach (var l in cusTagList)
                                {
                                    var lModel = new WJewel.DataContract.CRM.Customer.CustomerTags()
                                    {
                                        Id = l.Id,
                                        LabelId = l.LabelId
                                    };
                                    if (labelList != null && labelList.Count > 0)
                                    {
                                        var cusLabel = labelList.Find(s => s.Id == l.LabelId);
                                        if (cusLabel != null)
                                            lModel.Name = cusLabel.LabelName;
                                    }
                                    model.LabelList.Add(lModel);
                                }
                            }
                        }
                        if (userList != null && userList.Count > 0)
                        {
                            var user = userList.Find(s => s.Mobile == t.CusPhoneNo);
                            if (user != null && !string.IsNullOrWhiteSpace(user.OpenId))
                            {
                                model.OpenId = user.OpenId;
                                result.ReturnData.Add(model);
                            }
                        }
                    }
                }
                result.Success = true;

            }
            catch (Exception ex)
            {
                result.Message = "出现错误";
                return result;
            }
            return result;
        }


        /// <summary>
        /// 删除跟进人信息
        /// </summary>
        /// <param name="ud"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public static OperatResult<bool> DeleteFollow(UserData ud, string staffId)
        {
            OperatResult<bool> result = new OperatResult<bool>() { Success = false, ReturnData = false };
            try
            {
                string[] customerIds = new string[] { };
                var customerList = new Custom().GetListByStaffId(staffId);//查找该员工跟进的客户
                if (customerList != null && customerList.Count > 0)
                {
                    customerIds = customerList.Select(s => s.Id).ToArray();
                }
                var ret = new Custom().ModifyFollowPerson(customerIds, staffId);
                if (ret)
                {
                    result.Success = true;
                    result.ReturnData = true;
                    result.Message = "删除跟进人成功";
                }
            }
            catch (Exception ex)
            {
                result.Message = "出现错误";
                return result;
            }
            return result;
        }

    }
}

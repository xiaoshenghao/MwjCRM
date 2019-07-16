using CRM.Customer.Service;
using Microsoft.Web.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WJewel.Basic;
using WJewel.Basic.Common;
using WJewel.Basic.Helper;
using WJewel.DataContract.CRM.Customer;
using WJewel.DataContract.Common;
using WJewel.DataContract.CRM.Report;
using WJewel.DataContract.CRM.Wisdom;

namespace CRM.Customer.API.Controllers.V1._000
{
    /// <summary>
    /// 客户接口
    /// </summary>
    [ApiVersion("1.0")]
    [RoutePrefix("v{api-version:apiVersion}/Api/Customer")]
    public class CustomerController : BaseController
    {
        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("PgaeInit")]
        public ReturnResult<CustomerInit> PgaeInit()
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<CustomerInit>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<CustomerInit>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.PageInit(WorkContext);
                if (result.Success)
                    return new ReturnResult<CustomerInit>((int)ErrorCodeEnum.Success, result.ReturnData, "页面初始化");
                else
                    return new ReturnResult<CustomerInit>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<CustomerInit>((int)ErrorCodeEnum.Failed, null, ex.Tostring());
            }
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Create")]
        public ReturnResult<bool> Create(CreateCustomer model)
        {
            if (model == null || WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            if (string.IsNullOrWhiteSpace(model.CusName) || string.IsNullOrWhiteSpace(model.CusPhoneNo) || model.CusRegisterTime == null)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "参数错误，客户名称、电话、注册时间不能为空");
            if (model.CusCurrentScore < 0)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "参数错误，初始积分必须大于等于0");
            var result = CustomerService.Create(model, WorkContext);
            if (result.Success)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Success, true, "新增客户成功");
            else
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("Modify")]
        public ReturnResult<bool> Modify(ModifyCustomer model)
        {
            if (model == null || WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            if (string.IsNullOrWhiteSpace(model.CusName) || string.IsNullOrWhiteSpace(model.CusPhoneNo))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "参数错误，客户名称、电话不能为空");
            var result = CustomerService.Modify(model, WorkContext);
            if (result.Success)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Success, true, "编辑客户成功");
            else
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
        }

        /// <summary>
        /// 批量贴客户标签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("ModifyCustomerTags")]
        public ReturnResult ModifyCustomerTags(ModifyCustomerTags model)
        {
            if (model == null || WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            if (model.customerIds == null || model.customerIds.Count == 0 || model.labelIds == null || model.labelIds.Count == 0)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "参数错误");
            var result = CustomerService.ModifyCustomerTags(model, WorkContext);
            if (result.Success)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Success, true, "编辑成功");
            else
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
        }

        /// <summary>
        /// 批量修改客户标签，去掉以前赋值新的
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("ModifyCustomerTagsForCrm")]
        public ReturnResult ModifyCustomerTagsForCrm(ModifyCustomerTags model)
        {
            if (model == null || WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            if (model.customerIds == null || model.customerIds.Count == 0)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "参数错误");
            var result = CustomerService.ModifyCustomerTagsForCrm(model, WorkContext);
            if (result.Success)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Success, true, "编辑成功");
            else
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
        }

        /// <summary>
        /// 根据编号获取客户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("GetById")]
        public ReturnResult<CustomerModel> GetById(string id)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            if (string.IsNullOrWhiteSpace(id))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误");
            var result = CustomerService.GetById(id, WorkContext);
            if (result.Success)
                if (result.ReturnData == null)
                    return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
            else
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Failed, null, result.Message);
        }

        /// <summary>
        /// 根据编号获取客户信息（消息模版使用）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet, Route("GetByIdForMsg")]
        public ReturnResult<CustomerModel> GetByIdForMsg(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误");
            var result = CustomerService.GetByIdForMsg(id, WorkContext);
            if (result.Success)
                if (result.ReturnData == null)
                    return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
            else
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Failed, null, result.Message);
        }

        /// <summary>
        /// 根据客户手机号获取客户信息
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet, Route("GetShareByPhone")]
        public ReturnResult<CustomerModel> GetShareByPhone(string phone)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            if (string.IsNullOrWhiteSpace(phone))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误");
            var result = CustomerService.GetShareByPhone(phone, WorkContext);
            if (result.Success)
                if (result.ReturnData == null)
                    return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
            else
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Failed, null, result.Message);
        }

        /// <summary>
        /// 根据人脸Id编号获取客户信息
        /// </summary>
        /// <param name="faceId"></param>
        /// <returns></returns>
        [HttpGet, Route("GetByFaceId")]
        [AllowAnonymous]
        public ReturnResult<CustomerModel> GetByFaceId(int faceId)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            if (faceId <= 0)
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误");
            var result = CustomerService.GetByFaceId(faceId, WorkContext);
            if (result.Success)
                if (result.ReturnData == null)
                    return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
            else
                return new ReturnResult<CustomerModel>((int)ErrorCodeEnum.Failed, null, result.Message);
        }

        /// <summary>
        /// 根据人脸Id编号数组获取客户列表信息
        /// </summary>
        /// <param name="model">搜索条件实体</param>
        /// <returns></returns>
        [HttpPost, Route("GetListByFaceIds")]
        [AllowAnonymous]
        public ReturnResult<List<CustomerModel>> GetListByFaceIds([FromBody]CustomerListSearchModel model)
        {
            var result = CustomerService.GetListByFaceIds(model.FaceIds);
            if (result.Success)
                if (result.ReturnData == null)
                    return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
            else
                return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
        }

        /// <summary>
        /// 获取客户列表
        /// </summary>
        /// <param name="levelId">会员等级编号</param>
        /// <param name="vague">客户名称/客户号 模糊搜索</param>
        /// <param name="cusName">单客户名称模糊搜索</param>
        /// <param name="cusNo">单客户号模糊搜索</param>
        /// <param name="phoneNo">客户手机模糊搜索</param>
        /// <param name="cusSource">客户来源</param>
        /// <param name="cusFollowPerson">跟进人编号</param>
        /// <param name="status">状态 -1：所有 0：失效 1：正常</param>
        /// <param name="isActivation">是否激活 -1：所有 0：未激活 1：激活</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet, Route("GetList")]
        public ReturnResult<PageList<CustomerListModel>> GetList(string levelId = "", string vague = "", string cusName = "", string cusNo = "", string phoneNo = "", string cusSource = "", string cusFollowPerson = "",
                int status = -1, int isActivation = -1, int pageIndex = 1, int pageSize = 15)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.GetList(WorkContext, levelId, vague, cusName, cusNo, phoneNo, cusSource, cusFollowPerson, status, isActivation, pageIndex, pageSize);
                if (result.Success)
                    if (result.ReturnData == null || result.ReturnData.DataList.Count == 0)
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 获取客户列表(门店组客户共享列表，有共享就组内门店的所有客户，没有就本门店的客户)
        /// </summary>
        /// <param name="levelId">会员等级编号</param>
        /// <param name="vague">客户名称/客户号 模糊搜索</param>
        /// <param name="cusName">单客户名称模糊搜索</param>
        /// <param name="cusNo">单客户号模糊搜索</param>
        /// <param name="phoneNo">客户手机模糊搜索</param>
        /// <param name="cusSource">客户来源</param>
        /// <param name="cusFollowPerson">跟进人编号</param>
        /// <param name="status">状态 -1：所有 0：失效 1：正常</param>
        /// <param name="isActivation">是否激活 -1：所有 0：未激活 1：激活</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet, Route("GetShareList")]
        public ReturnResult<PageList<CustomerListModel>> GetShareList(string levelId = "", string vague = "", string cusName = "", string cusNo = "", string phoneNo = "", string cusSource = "", string cusFollowPerson = "",
                int status = -1, int isActivation = -1, int pageIndex = 1, int pageSize = 15)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.GetShareList(WorkContext, levelId, vague, cusName, cusNo, phoneNo, cusSource, cusFollowPerson, status, isActivation, pageIndex, pageSize);
                if (result.Success)
                    if (result.ReturnData == null || result.ReturnData.DataList.Count == 0)
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 根据id列表获取客户列表(门店组客户共享列表，有共享就组内门店的所有客户，没有就本门店的客户)
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetShareListByIds")]
        public ReturnResult<List<CustomerBaseModel>> GetShareListByIds(IdListModel model)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.UserID))
                return new ReturnResult<List<CustomerBaseModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (model == null || model.IdList == null | model.IdList.Count < 1)
                return new ReturnResult<List<CustomerBaseModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "缺少参数");
            try
            {
                var result = CustomerService.GetShareListByIds(WorkContext, model.IdList);
                if (result.Success)
                    if (result.ReturnData == null || result.ReturnData.Count == 0)
                        return new ReturnResult<List<CustomerBaseModel>>((int)ErrorCodeEnum.Error_NoData, null, "没有数据");
                    else
                        return new ReturnResult<List<CustomerBaseModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<List<CustomerBaseModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<List<CustomerBaseModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }


        /// <summary>
        /// 获取客户列表(门店组客户共享列表，有共享就组内门店的所有客户，没有就本门店的客户,CRM员工通道用)
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetShareListByCrm")]
        public ReturnResult<PageList<CustomerListModel>> GetShareListByCrm([FromBody]GetShareListByCrmPar par)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.GetShareListByCrm(WorkContext, par.LevelIds, par.IsBindWx, par.isActivation, par.keyWord, par.pageIndex, par.pageSize);
                if (result.Success)
                    if (result.ReturnData == null || result.ReturnData.DataList.Count == 0)
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }


        /// <summary>
        /// 获取客户列表(门店组客户共享列表，有共享就组内门店的所有客户，没有就本门店的客户)
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetShareCusList")]
        public ReturnResult<PageList<CustomerListModel>> GetShareCusList([FromBody]CustomerListSearchParamsModel paramModel)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.GetShareList(paramModel, WorkContext);
                if (result.Success)
                    if (result.ReturnData == null || result.ReturnData.DataList.Count == 0)
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }


        /// <summary>
        /// 获取客户下拉（状态正常的客户）
        /// </summary>
        /// <param name="cusName">单客户名称模糊搜索</param>
        /// <param name="phoneNo">客户手机模糊搜索</param>
        /// <returns></returns>
        [HttpGet, Route("GetDropDown")]
        public ReturnResult<List<CustomerDropDown>> GetDropDown(string cusName = "", string phoneNo = "")
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.GetDropDown(WorkContext, cusName, phoneNo, 1);
                if (result.Success)
                    if (result.ReturnData == null || result.ReturnData.Count == 0)
                        return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 获取客户下拉（所有客户）
        /// </summary>
        /// <param name="cusName">单客户名称模糊搜索</param>
        /// <param name="phoneNo">客户手机模糊搜索</param>
        /// <returns></returns>
        [HttpGet, Route("GetAllDropDown")]
        public ReturnResult<List<CustomerDropDown>> GetAllDropDown(string cusName = "", string phoneNo = "")
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.GetDropDown(WorkContext, cusName, phoneNo, -1);
                if (result.Success)
                    if (result.ReturnData == null || result.ReturnData.Count == 0)
                        return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<List<CustomerDropDown>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }
        

        /// <summary>
        /// 员工离职后更新跟进人信息
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpGet, Route("ModifyFollowStatus")]
        public ReturnResult<bool> ModifyFollowStatus(string staffId)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            try
            {
                var result = CustomerService.ModifyFollowStatus(WorkContext, staffId);
                if (result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Success, true, "更新成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }

        /// <summary>
        /// 员工名称变更后后更新跟进人信息
        /// </summary>
        /// <param name="staffId"></param>
        /// <param name="staffName"></param>
        /// <returns></returns>
        [HttpGet, Route("ModifyFollowName")]
        public ReturnResult<bool> ModifyFollowName(string staffId, string staffName)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            try
            {
                var result = CustomerService.ModifyFollowName(WorkContext, staffId, staffName);
                if (result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Success, true, "更新成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }

        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("ModifyStatus")]
        public ReturnResult<bool> ModifyStatus(string id)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            if (string.IsNullOrWhiteSpace(id))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "参数错误");
            try
            {
                var result = CustomerService.ModifyStatus(id, WorkContext);
                if (result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Error_NoData, true, "编辑成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }
        
        /// <summary>
        /// 获取未跟进客户统计
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetStatistics")]
        public ReturnResult<List<NoFollowCustomStatistics>> GetStatistics()
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<List<NoFollowCustomStatistics>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<List<NoFollowCustomStatistics>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            var result = CustomerService.GetStatistics(WorkContext);
            if (result.Success)
                return new ReturnResult<List<NoFollowCustomStatistics>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
            else
                return new ReturnResult<List<NoFollowCustomStatistics>>((int)ErrorCodeEnum.Failed, result.ReturnData, result.Message);
        }

        /// <summary>
        /// 客户分配
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("StaffCustomerDis")]
        public ReturnResult<bool> StaffCustomerDis(StaffCustomerDistrbution model)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            if (model == null || model.StaffCusNumber == null || model.StaffCusNumber.Count <= 0)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "参数错误");
            try
            {
                var result = CustomerService.StaffCustomerDis(model.StaffCusNumber, WorkContext);
                if (result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Error_NoData, true, "分配成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }


        /// <summary>
        /// 客户进店分配跟进人,已存在跟进人，默认返回true
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("AssignmentFollowers")]
        public ReturnResult<bool> AssignmentFollowers(string cusId, string staffId)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "用户数据异常");
            if (string.IsNullOrWhiteSpace(cusId) || string.IsNullOrWhiteSpace(staffId))
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "参数错误，不能为空");
            try
            {
                var result = CustomerService.AssignmentFollowers(cusId, staffId, WorkContext);
                if (result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Error_NoData, true, "分配成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }


        /// <summary>
        /// 获取自动发放卡卷的用户信息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("GetAutomaticCusInfo")]
        public ReturnResult<List<CustomerInfo>> GetAutomaticCusInfo(AssignmentFollowersPar model)
        {
            if (model == null && string.IsNullOrWhiteSpace(model.mctNum) || model.storeIds == null)
                return new ReturnResult<List<CustomerInfo>>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误，不能为空");
            try
            {
                var result = CustomerService.GetAutomaticCusInfo(model.mctNum, model.storeIds, model.cusLevelIds, model.cusLabelIds, model.cusIds);
                if (result.Success)
                    return new ReturnResult<List<CustomerInfo>>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取");
                else
                    return new ReturnResult<List<CustomerInfo>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<List<CustomerInfo>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 根据门店列表跟客户手机号获取客户信息 微信创建卡卷回调的时候调用该方法获取客户信息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("GetCustomerByPhoneNoAndStoreIds")]
        public ReturnResult<CustomerInfo> GetCustomerByPhoneNoAndStoreIds(GetCustomerByPhoneNoAndStoreIdsModel model)
        {
            if (model == null || model.StoreIds == null)
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误，不能为空");
            try
            {
                var result = CustomerService.GetCustomerByPhoneNoAndStoreIds(model.StoreIds, model.CusPhoneNo);
                if (result.Success)
                    return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取");
                else
                    return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 根据商户号手机号获取列表 积分商城卡券列表调用
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet, Route("GetCustomerByPhoneNoAndMct")]
        public ReturnResult<List<CustomerInfo>> GetCustomerByPhoneNoAndMct(string mctNum, string phoneNo)
        {
            if (string.IsNullOrWhiteSpace(mctNum) || string.IsNullOrWhiteSpace(phoneNo))
                return new ReturnResult<List<CustomerInfo>>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误，不能为空");
            try
            {
                var result = CustomerService.GetCustomerByPhoneNoAndMct(mctNum, phoneNo);
                if (result.Success)
                    return new ReturnResult<List<CustomerInfo>>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取");
                else
                    return new ReturnResult<List<CustomerInfo>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<List<CustomerInfo>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 微信放开领券时适用门店没有客户信息时根据商户，客户手机获取一条客户记录插入到适用门店中
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet, Route("CreateCustomerByMctNumAndPhone")]
        public ReturnResult<CustomerInfo> CreateCustomerByMctNumAndPhone(string mctNum, string phoneNo, string storeId)
        {
            if (string.IsNullOrWhiteSpace(mctNum) || string.IsNullOrWhiteSpace(phoneNo))
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误，不能为空");
            try
            {
                var result = CustomerService.CreateCustomerByMctNumAndPhone(mctNum, phoneNo, storeId);
                if (result.Success)
                    return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取");
                else
                    return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 根据客户编号获取客户标签编号  卡券积分商城列表调用
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("GetCustomerTags")]
        public ReturnResult<List<TagsShopModel>> GetCustomerTags(IdListModel cusIds)
        {
            if (cusIds == null || cusIds.IdList == null || cusIds.IdList.Count == 0)
                return new ReturnResult<List<TagsShopModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误，不能为空");
            try
            {
                var result = CustomerService.GetCustomerTags(cusIds.IdList.ToArray());
                if (result.Success)
                    return new ReturnResult<List<TagsShopModel>>((int)ErrorCodeEnum.Error_NoData, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<List<TagsShopModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception)
            {
                return new ReturnResult<List<TagsShopModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        #region 微信H5使用
        /// <summary>
        /// 微信获取会员客户信息-微信H5使用 
        /// </summary>
        /// <param name="storeId">门店id</param>
        /// <returns></returns>
        [HttpGet, Route("GetCustomerUserInfoForWX")]
        public ReturnResult<CustomerUserForWXModel> GetCustomerUserInfo(string storeId = "")
        {
            if (WorkContext == null)
                return new ReturnResult<CustomerUserForWXModel>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            var mctNum = userMct.Find(s => s.PlatId == CommonPlatModel.Customer) != null ? userMct.Find(s => s.PlatId == CommonPlatModel.Customer).MctNum : string.Empty;
            try
            {
                var result = CustomerService.GetCustomerUserInfoForWX(WorkContext, mctNum, storeId);
                if (result != null && result.ReturnData != null && result.Success)
                    return new ReturnResult<CustomerUserForWXModel>((int)ErrorCodeEnum.Success, result.ReturnData, "微信获取会员客户信息成功");
                else
                    return new ReturnResult<CustomerUserForWXModel>((int)ErrorCodeEnum.Failed, null, result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "微信获取会员客户信息失败");
            }
            catch (Exception ex)
            {
                return new ReturnResult<CustomerUserForWXModel>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 微信根据手机号获取会员客户信息-微信H5使用 
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="storeId">门店id</param>
        /// <returns></returns>
        [HttpGet, Route("GetCustomerUserInfoByMobileForWX")]
        [AllowAnonymous]
        public ReturnResult<CustomerUserForWXModel> GetCustomerUserInfoByMobileForWX(string mobile, string storeId = "")
        {
            var mctNum = userMct.Find(s => s.PlatId == CommonPlatModel.Customer) != null ? userMct.Find(s => s.PlatId == CommonPlatModel.Customer).MctNum : string.Empty;
            try
            {
                var result = CustomerService.GetCustomerUserInfoByMobileForWX(mobile, storeId);
                if (result != null && result.ReturnData != null && result.Success)
                    return new ReturnResult<CustomerUserForWXModel>((int)ErrorCodeEnum.Success, result.ReturnData, "微信获取会员客户信息成功");
                else
                    return new ReturnResult<CustomerUserForWXModel>((int)ErrorCodeEnum.Failed, null, result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "微信获取会员客户信息失败");
            }
            catch (Exception ex)
            {
                return new ReturnResult<CustomerUserForWXModel>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 微信修改会员客户信息-微信H5使用
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("ModifyForWX")]
        public ReturnResult<bool> ModifyForWX([FromBody]CustomerUserModifyForWXModel model)
        {
            if (WorkContext == null)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            var mctNum = userMct.Find(s => s.PlatId == CommonPlatModel.Customer) != null ? userMct.Find(s => s.PlatId == CommonPlatModel.Customer).MctNum : string.Empty;
            try
            {
                var result = CustomerService.ModifyForWX(WorkContext, model, mctNum);
                if (result != null && result.ReturnData && result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Success, result.ReturnData, "修改信息成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "修改信息失败");
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }
        /// <summary>
        /// 微信修改客户会员号信息-微信激活卡券使用
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("ModifyCardForWX")]
        [AllowAnonymous]
        public ReturnResult<bool> ModifyCardForWX([FromBody]CustomerModifyCardNoModel model)
        {
            //if (WorkContext == null)
            //    return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            //var mctNum = userMct.Find(s => s.PlatId == CommonPlatModel.Customer) != null ? userMct.Find(s => s.PlatId == CommonPlatModel.Customer).MctNum : string.Empty;
            try
            {
                var result = CustomerService.ModifyCardForWX(model);
                if (result != null && result.ReturnData && result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Success, result.ReturnData, "修改信息成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "修改信息失败");
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }

        /// <summary>
        /// 微信会员更新手机号前的验证
        /// </summary>
        /// <param name="phoneNo">新手机号</param>
        /// <returns></returns>
        [HttpGet, Route("CheckModifyMobile")]
        public ReturnResult<bool> CheckModifyMobile(string phoneNo)
        {
            if (WorkContext == null)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            var mctNum = userMct.Find(s => s.PlatId == CommonPlatModel.Customer) != null ? userMct.Find(s => s.PlatId == CommonPlatModel.Customer).MctNum : string.Empty;
            try
            {
                var result = CustomerService.CheckModifyMobile(WorkContext, phoneNo);
                if (result != null && result.ReturnData && result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Success, result.ReturnData, "验证成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "验证失败");
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }

        /// <summary>
        /// 微信会员卡激活后激活客户信息-微信H5使用
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("ActivityForWX")]
        [AllowAnonymous]
        public ReturnResult<string> ActivityForWX([FromBody]ActivityCustomerForWXModel model)
        {
            try
            {
                if (model == null)
                {
                    return new ReturnResult<string>((int)ErrorCodeEnum.Parameter_Missing, null, "激活客户信息失败,激活客户信息不能为空");
                }
                if (string.IsNullOrEmpty(model.MctCode))
                {
                    return new ReturnResult<string>((int)ErrorCodeEnum.Failed, null, "激活客户信息失败,请提供商户编码");
                }
                if (string.IsNullOrEmpty(model.MctNum))
                {
                    return new ReturnResult<string>((int)ErrorCodeEnum.Failed, null, "激活客户信息失败,请提供商户号");
                }
                if (string.IsNullOrEmpty(model.PhoneNo))
                {
                    return new ReturnResult<string>((int)ErrorCodeEnum.Failed, null, "激活客户信息失败,请提供用户手机号");
                }

                var result = CustomerService.ActivityForWX(model);
                if (result != null && result.ReturnData != null && result.Success)
                    return new ReturnResult<string>((int)ErrorCodeEnum.Success, result.ReturnData, "激活客户信息成功");
                else
                    return new ReturnResult<string>((int)ErrorCodeEnum.Failed, null, result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "激活客户信息失败");
            }
            catch (Exception ex)
            {
                return new ReturnResult<string>((int)ErrorCodeEnum.Failed, null, "激活客户信息失败,程序出错");
            }
        }
        #endregion

        #region 统计报表使用
        /// <summary>
        /// 获取按等级统计客户列表
        /// </summary>
        /// <param name="storeId">门店Id</param>
        /// <param name="beginRegistedDate">注册时间开始</param>
        /// <param name="endRegistedDate">注册时间截止</param>
        /// <returns></returns>
        [HttpGet, Route("GetListByLevel")]
        public ReturnResult<List<GetListByLevelModel>> GetListByLevel(string storeId = "", string beginRegistedDate = "", string endRegistedDate = "")
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<List<GetListByLevelModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<List<GetListByLevelModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.GetListByLevel(WorkContext, storeId, beginRegistedDate, endRegistedDate);
                if (result != null && result.Success)
                    if (result.ReturnData == null || result.ReturnData.Count == 0)
                        return new ReturnResult<List<GetListByLevelModel>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<List<GetListByLevelModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<List<GetListByLevelModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<List<GetListByLevelModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }
        /// <summary>
        /// 获取商户下客户列表
        /// </summary>
        /// <param name="storeId">门店Id</param>
        /// <param name="beginRegistedDate">注册时间开始</param>
        /// <param name="endRegistedDate">注册时间截止</param>
        /// <returns></returns>
        [HttpGet, Route("GetAllList")]
        public ReturnResult<List<CustomerModel>> GetAllList(string storeId = "", string beginRegistedDate = "", string endRegistedDate = "")
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.GetAllList(WorkContext, storeId, beginRegistedDate, endRegistedDate);
                if (result != null && result.Success)
                    if (result.ReturnData == null || result.ReturnData.Count == 0)
                        return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<List<CustomerModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }
        #endregion

        /// <summary>
        /// 获取客户在卡券适用门店中最高等级权重 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("IsCanPullCardRoll")]
        public ReturnResult<CustomerInfo> IsCanPullCardRoll([FromBody]PullCardRollModel model)
        {
            if (WorkContext == null)
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (model == null)
            {
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误，不能为空");
            }
            if (model.StoreIds == null || !model.StoreIds.Any())
            {
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Parameter_Missing, null, "请填写适用门店");
            }
            if (string.IsNullOrWhiteSpace(model.Mobile))
            {
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Parameter_Missing, null, "请填写客户手机号");
            }
            var mctNum = userMct.Find(s => s.PlatId == CommonPlatModel.Customer) != null ? userMct.Find(s => s.PlatId == CommonPlatModel.Customer).MctNum : string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(mctNum))
                {
                    WorkContext.MctNum = mctNum;
                }
                var result = CustomerService.IsCanPullCardRoll(WorkContext, model);
                if (result.Success)
                    return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<CustomerInfo>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }

        /// <summary>
        /// 根据客户积分获取符合的门店列表
        /// </summary>
        /// <param name="integral">积分</param>
        /// <returns></returns>
        [HttpGet, Route("GetStoreListByIntegral")]
        public ReturnResult<List<DropDownModel>> GetStoreListByIntegral(int integral)
        {
            if (WorkContext == null)
                return new ReturnResult<List<DropDownModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (integral <= 0)
            {
                return new ReturnResult<List<DropDownModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "参数错误");
            }
            var mctNum = userMct.Find(s => s.PlatId == CommonPlatModel.Customer) != null ? userMct.Find(s => s.PlatId == CommonPlatModel.Customer).MctNum : string.Empty;
            if (string.IsNullOrWhiteSpace(mctNum))
            {
                return new ReturnResult<List<DropDownModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "未找到上级商户信息");
            }
            try
            {
                var result = CustomerService.GetStoreListByIntegral(WorkContext, mctNum, integral);
                if (result.Success)
                {
                    if (result.ReturnData == null)
                    {
                        return new ReturnResult<List<DropDownModel>>((int)ErrorCodeEnum.Error_NoData, null, "暂无数据");
                    }
                    return new ReturnResult<List<DropDownModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                }
                else
                    return new ReturnResult<List<DropDownModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<List<DropDownModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }


        /// <summary>
        /// 编辑客户微信卡包信息
        /// </summary>
        /// <param name="customerId">客户id</param>
        /// <param name="integral">变动的积分</param>
        /// <returns></returns>
        [HttpGet, Route("ModifyWxCardInfo")]
        public ReturnResult<bool> ModifyWxCardInfo(string customerId, int integral)
        {
            try
            {
                if (WorkContext == null)
                {
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Error_NoPermission, false, "请先登录系统");
                }

                if (string.IsNullOrEmpty(WorkContext.Account) || string.IsNullOrEmpty(WorkContext.UserID))
                {
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Error_NoPermission, false, "您当前的登录帐号异常");
                }
                var ret = CustomerService.ModifyWxCardInfo(WorkContext, customerId, integral);
                if (ret != null && ret.Success)
                {
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Success, ret.Success, "客户微信卡包信息编辑成功！");
                }
                else
                {
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, ret.Message);
                }
            }
            catch (Exception ex)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Error_Program, false, ex.Message);
            }
        }

        /// <summary>
        /// 获取会员数据分析
        /// </summary>
        /// <param name="storeIds">门店编号，可多选</param>
        /// <param name="dateStart">查询开始日期，yyyy-MM-dd格式</param>
        /// <param name="dateEnd">查询结束日期，yyyy-MM-dd格式</param>
        /// <returns>积分数据分析结果</returns>
        [HttpGet, Route("GetCustomerStatistics")]
        public ReturnResult<CustomerAnalysisModel> GetCustomerStatistics(string dateStart, string dateEnd, string storeIds = "")
        {
            if (string.IsNullOrEmpty(storeIds))
            {
                storeIds = WorkContext.StoreId;
            }
            var date = DateTime.Now;
            if (!DateTime.TryParse(dateStart, out date))
            {
                return new ReturnResult<CustomerAnalysisModel>((int)ErrorCodeEnum.Error_NoPermission, null, "查询开始日期格式不对");
            }
            if (!DateTime.TryParse(dateEnd, out date))
            {
                return new ReturnResult<CustomerAnalysisModel>((int)ErrorCodeEnum.Error_NoPermission, null, "查询结束日期格式不对");
            }

            try
            {
                if (WorkContext == null)
                {
                    return new ReturnResult<CustomerAnalysisModel>((int)ErrorCodeEnum.Error_NoPermission, null, "请先登录系统");
                }

                if (string.IsNullOrEmpty(WorkContext.Account) || string.IsNullOrEmpty(WorkContext.UserID))
                {
                    return new ReturnResult<CustomerAnalysisModel>((int)ErrorCodeEnum.Error_NoPermission, null, "您当前的登录帐号异常");
                }

                var ret = CustomerService.CustomerStatistics(WorkContext, storeIds, dateStart, dateEnd);
                if (ret != null && ret.Success)
                {
                    return new ReturnResult<CustomerAnalysisModel>((int)ErrorCodeEnum.Success, ret.ReturnData, "积分数据统计成功");
                }
                else
                {
                    return new ReturnResult<CustomerAnalysisModel>((int)ErrorCodeEnum.Failed, null, ret != null && !string.IsNullOrEmpty(ret.Message) ? ret.Message : "积分数据统计失败！");
                }
            }
            catch (Exception ex)
            {
                return new ReturnResult<CustomerAnalysisModel>((int)ErrorCodeEnum.Error_Program, null, ex.Message);
            }
        }
        /// <summary>
        /// 僵尸粉分析
        /// </summary>
        /// <param name="mode">查询类型1、三个月，2、半年、3一年</param>
        /// <param name="storeIds">门店编号，可多选</param>
        /// <param name="regStart">注册开始日期，yyyy-MM-dd格式</param>
        /// <param name="regEnd">注册结束日期，yyyy-MM-dd格式</param>
        /// <returns>积分数据分析结果</returns>
        [HttpGet, Route("GetZombieCustomerStatistics")]
        public ReturnResult<List<ZombiesStatisticsModel>> GetZombieCustomerStatistics(int mode, string regStart, string regEnd, string storeIds = "")
        {
            if (string.IsNullOrEmpty(storeIds))
            {
                storeIds = WorkContext.StoreId;
            }
            var date = DateTime.Now;
            if (!DateTime.TryParse(regStart, out date))
            {
                return new ReturnResult<List<ZombiesStatisticsModel>>((int)ErrorCodeEnum.Error_NoPermission, null, "注册开始日期格式不对");
            }
            if (!DateTime.TryParse(regEnd, out date))
            {
                return new ReturnResult<List<ZombiesStatisticsModel>>((int)ErrorCodeEnum.Error_NoPermission, null, "注册结束日期格式不对");
            }

            try
            {
                if (WorkContext == null)
                {
                    return new ReturnResult<List<ZombiesStatisticsModel>>((int)ErrorCodeEnum.Error_NoPermission, null, "请先登录系统");
                }

                if (string.IsNullOrEmpty(WorkContext.Account) || string.IsNullOrEmpty(WorkContext.UserID))
                {
                    return new ReturnResult<List<ZombiesStatisticsModel>>((int)ErrorCodeEnum.Error_NoPermission, null, "您当前的登录帐号异常");
                }

                var ret = CustomerService.ZombieCustomerStatistics(WorkContext, mode, regStart, regEnd, storeIds);
                if (ret != null && ret.Success)
                {
                    return new ReturnResult<List<ZombiesStatisticsModel>>((int)ErrorCodeEnum.Success, ret.ReturnData, "僵尸粉分析成功");
                }
                else
                {
                    return new ReturnResult<List<ZombiesStatisticsModel>>((int)ErrorCodeEnum.Failed, null, ret != null && !string.IsNullOrEmpty(ret.Message) ? ret.Message : "僵尸粉分析失败！");
                }
            }
            catch (Exception ex)
            {
                return new ReturnResult<List<ZombiesStatisticsModel>>((int)ErrorCodeEnum.Error_Program, null, ex.Message);
            }
        }
        /// <summary>
        /// 获取僵尸粉明细列表
        /// </summary>
        /// <param name="gradeId">客户等级</param>
        /// <param name="mode">查询类型1、三个月，2、半年、3一年</param>
        /// <param name="storeIds">门店编号，可多选</param>
        /// <param name="regStart">注册开始日期，yyyy-MM-dd格式</param>
        /// <param name="regEnd">注册结束日期，yyyy-MM-dd格式</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数</param>
        /// <returns></returns>
        [HttpGet, Route("GetZombieCustomerList")]
        public ReturnResult<PageList<CustomerListModel>> GetZombieCustomerList(string gradeId, int mode, string regStart, string regEnd, string storeIds, int pageIndex = 1, int pageSize = 15)
        {
            if (WorkContext == null || string.IsNullOrEmpty(WorkContext.MctNum))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "登录异常");
            if (string.IsNullOrWhiteSpace(WorkContext.StoreId))
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Parameter_Missing, null, "用户数据异常");
            try
            {
                var result = CustomerService.ZombiesCustomDetails(WorkContext, gradeId, mode, regStart, regEnd, storeIds, pageIndex, pageSize);
                if (result.Success)
                    if (result.ReturnData == null || result.ReturnData.DataList.Count == 0)
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Error_NoData, null, "获取成功");
                    else
                        return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Success, result.ReturnData, "获取成功");
                else
                    return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, result.Message);
            }
            catch (Exception ex)
            {
                return new ReturnResult<PageList<CustomerListModel>>((int)ErrorCodeEnum.Failed, null, "程序出错");
            }
        }


        /// <summary>
        /// 删除员工跟进的信息
        /// </summary>
        /// <param name="staffId">员工id</param>
        /// <returns></returns>
        [HttpGet, Route("DeleteFollow")]
        public ReturnResult<bool> DeleteFollow(string staffId)
        {
            if (WorkContext == null)
                return new ReturnResult<bool>((int)ErrorCodeEnum.Parameter_Missing, false, "登录异常");
            try
            {
                var result = CustomerService.DeleteFollow(WorkContext, staffId);
                if (result != null && result.ReturnData && result.Success)
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Success, result.ReturnData, "操作成功");
                else
                    return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "操作成功");
            }
            catch (Exception)
            {
                return new ReturnResult<bool>((int)ErrorCodeEnum.Failed, false, "程序出错");
            }
        }
    }
}

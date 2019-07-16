using System;
using System.Linq;
using System.Text;

namespace CRM.Customer.Domain
{
    ///<summary>
    ///
    ///</summary>
    public partial class Cus_Customer
    {
        public Cus_Customer()
        {


        }
        /// <summary>
        /// Desc:编号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string MctNum { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string StoreId { get; set; }

        /// <summary>
        /// Desc:状态
        /// Default:b'1'
        /// Nullable:False
        /// </summary>           
        public bool Status { get; set; }

        /// <summary>
        /// Desc:是否已激活
        /// Default:b'0'
        /// Nullable:False
        /// </summary>
        public bool IsActivation { get; set; }

        /// <summary>
        /// Desc:客户头像
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusLogo { get; set; }

        /// <summary>
        /// Desc:客户微信头像
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusWxLogo { get; set; }

        /// <summary>
        /// Desc:客户人脸编号
        /// Default:
        /// Nullable:True
        /// </summary>
        public int CusFaceId { get; set; }

        /// <summary>
        /// Desc:客户名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusName { get; set; }

        /// <summary>
        /// Desc:性别  0：女 1：男
        /// Default:b'0'
        /// Nullable:True
        /// </summary>           
        public bool CusSex { get; set; }

        /// <summary>
        /// Desc:客户号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string CusNo { get; set; }

        /// <summary>
        /// Desc:客户等级编码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusLevelId { get; set; }

        /// <summary>
        /// Desc:原会员号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusOldMemberNo { get; set; }

        /// <summary>
        /// Desc:电话号码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusPhoneNo { get; set; }

        /// <summary>
        /// Desc:注册时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CusRegisterTime { get; set; }

        /// <summary>
        /// Desc:客户来源
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusSource { get; set; }

        /// <summary>
        /// Desc:出生年月日
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CusBirthday { get; set; }

        /// <summary>
        /// Desc:所在区域
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusLocation { get; set; }

        /// <summary>
        /// Desc:省编号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusProvinceId { get; set; }

        /// <summary>
        /// Desc:省名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusProvinceName { get; set; }

        /// <summary>
        /// Desc:市编号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusCityId { get; set; }

        /// <summary>
        /// Desc:市名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusCityName { get; set; }

        /// <summary>
        /// Desc:区/县 编号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusAreaId { get; set; }

        /// <summary>
        /// Desc:区/县 名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusAreaName { get; set; }

        /// <summary>
        /// Desc:联系地址
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusAddress { get; set; }

        /// <summary>
        /// Desc:客户微信号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusWechatNo { get; set; }

        /// <summary>
        /// 是否已绑定微信
        /// </summary>
        public bool CusIsBindWX { get; set; }

        /// <summary>
        /// Desc:备注
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusRemark { get; set; }

        /// <summary>
        /// Desc:客户初始积分
        /// Default:0.00
        /// Nullable:True
        /// </summary>
        public decimal CusInitialIntegral { get; set; }

        /// <summary>
        /// Desc:客户累计积分
        /// Default:0.00
        /// Nullable:True
        /// </summary>           
        public decimal? CusAccumulatedPoints { get; set; }

        /// <summary>
        /// Desc:客户当前积分
        /// Default:0.00
        /// Nullable:True
        /// </summary>           
        public decimal? CusCurrentScore { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateAccount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateUserId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string LastModifyAccount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string LastModifyUserId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? LastModifyTime { get; set; }

        /// <summary>
        /// Desc:是否删除
        /// Default:0
        /// Nullable:True
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// Desc:跟进人id
        /// Default:0
        /// Nullable:True
        /// </summary>
        public string CusFollowPerson { get; set; }

    }

    ///<summary>
    ///
    ///</summary>
    public partial class V_Cus_Customer
    {
        /// <summary>
        /// Desc:编号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string MctNum { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string StoreId { get; set; }

        /// <summary>
        /// Desc:状态
        /// Default:b'1'
        /// Nullable:False
        /// </summary>           
        public bool Status { get; set; }

        /// <summary>
        /// Desc:是否已激活
        /// Default:b'0'
        /// Nullable:False
        /// </summary>
        public bool IsActivation { get; set; }

        /// <summary>
        /// Desc:客户头像
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusLogo { get; set; }

        /// <summary>
        /// Desc:客户名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusName { get; set; }

        /// <summary>
        /// Desc:性别  0：女 1：男
        /// Default:b'0'
        /// Nullable:True
        /// </summary>           
        public bool CusSex { get; set; }

        /// <summary>
        /// Desc:客户号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string CusNo { get; set; }

        /// <summary>
        /// Desc:客户等级编码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusLevelId { get; set; }

        /// <summary>
        /// Desc:电话号码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusPhoneNo { get; set; }

        /// <summary>
        /// Desc:注册时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CusRegisterTime { get; set; }

        /// <summary>
        /// Desc:出生年月日
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CusBirthday { get; set; }

        /// <summary>
        /// Desc:客户微信号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusWechatNo { get; set; }

        /// <summary>
        /// Desc:备注
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusRemark { get; set; }

        /// <summary>
        /// Desc:客户累计积分
        /// Default:0.00
        /// Nullable:True
        /// </summary>           
        public decimal? CusAccumulatedPoints { get; set; }

        /// <summary>
        /// Desc:客户当前积分
        /// Default:0.00
        /// Nullable:True
        /// </summary>           
        public decimal? CusCurrentScore { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Desc:客户等级名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CusLevelName { get; set; }
        /// <summary>
        /// 生日消费
        /// </summary>
        public decimal? BirthdayConsumption { get; set; }
        /// <summary>
        /// 普通消费
        /// </summary>
        public decimal? OrdinaryConsumption { get; set; }
        /// <summary>
        /// 其他权益
        /// </summary>
        public string OtherEquity { get; set; }
        /// <summary>
        /// 产品优惠
        /// </summary>
        public string ProductOffer { get; set; }
        /// <summary>
        /// 初始积分
        /// </summary>
        public decimal CusInitialIntegral { get; set; }
        /// <summary>
        /// 是否绑定微信
        /// </summary>
        public bool CusIsBindWX { get; set; }
        /// <summary>
        /// 最后订单时间
        /// </summary>
        public DateTime? LastConsumeTime { get; set; }

    }
}

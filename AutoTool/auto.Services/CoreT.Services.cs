﻿














//引入命名空间









 


//--------------------------------------------------------------------
//     此代码由T4模板自动生成
//	   生成时间 2019-10-01 23:34:26 
//     对此文件的更改可能会导致不正确的行为，并且如果重新生成代码，这些更改将会丢失。
//--------------------------------------------------------------------





	//----------IdentityUser开始----------
    

using System;
using CoreT.IServices;
using CoreT.IRepository;
using CoreT.Entity;

namespace CoreT.Services
{	
	/// <summary>
	/// IdentityUserServices
	/// </summary>	
	public class IdentityUserServices : BaseServices<IdentityUser>, IIdentityUserServices
    {
        IIdentityUserRepository dal;
        public IdentityUserServices(IIdentityUserRepository dal)
        {
            this.dal = dal;
            base.BaseDal = dal;
        }
    }
}

	//----------IdentityUser结束----------
	 


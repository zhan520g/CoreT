
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
	
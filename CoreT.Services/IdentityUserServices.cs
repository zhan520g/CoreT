
	//----------IdentityUser开始----------
    

using System;
using CoreT.FrameWork.IServices;
using CoreT.FrameWork.IRepository;
using CoreT.FrameWork.Entity;

namespace CoreT.FrameWork.Services
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
            base.baseDal = dal;
        }
       
    }
}

	//----------IdentityUser结束----------
	
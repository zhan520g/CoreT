
	//----------IdentityUser开始----------
    

using System;
using Blog.Core.FrameWork.IServices;
using Blog.Core.FrameWork.IRepository;
using Blog.Core.FrameWork.Entity;

namespace Blog.Core.FrameWork.Services
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
	
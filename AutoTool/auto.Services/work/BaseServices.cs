
    
	//----------开始----------
	
using System;
using Blog.Core.FrameWork.Entity;
using Blog.Core.FrameWork.IServices;
using Blog.Core.FrameWork.IRepository;
namespace Blog.Core.FrameWork.Services
{	
	/// <summary>
	/// IBaseRepository
	/// </summary>	
	public class BaseServices<TEntity> : IBaseServices<TEntity> where TEntity : class, new()
    {
		public IBaseRepository<TEntity> baseDal;
       
    }
}

	//----------结束----------
	
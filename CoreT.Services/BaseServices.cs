
    
	//----------开始----------
	
using System;
using CoreT.FrameWork.Entity;
using CoreT.FrameWork.IServices;
using CoreT.FrameWork.IRepository;
namespace CoreT.FrameWork.Services
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
	
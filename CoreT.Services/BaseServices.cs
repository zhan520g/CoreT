
    
	//----------开始----------
	
using System;
using CoreT.Entity;
using CoreT.IServices;
using CoreT.IRepository;
namespace CoreT.Services
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
	
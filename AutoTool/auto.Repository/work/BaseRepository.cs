
    
	//----------开始----------
	
using System;
using Blog.Core.FrameWork.Entity;
using Blog.Core.FrameWork.IRepository;
namespace Blog.Core.FrameWork.Repository
{	
	/// <summary>
	/// IBaseRepository
	/// </summary>	
	 public  class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
    {

       
    }
}

	//----------结束----------
	
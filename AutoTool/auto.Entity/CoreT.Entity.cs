//如果要获取主机信息，记得把 hostspecific 设置成true



//导入命名空间组件













//引入我们的公共模板文件

//引入命名空间









//定义我们的输出文件夹


//--------------------------------------------------------------------
//     此代码由T4模板自动生成
//	   生成时间 2019-10-01 21:50:33 
//     对此文件的更改可能会导致不正确的行为，并且如果重新生成代码，这些更改将会丢失。
//--------------------------------------------------------------------


	//连接数据库，打开 connect 连接

 //遍历全部数据库表
   

	//开始启动block块，参数是实体类文件名
    
	//----------IdentityUser开始----------
    
using System;
namespace CoreT.Entity
{	
	/// <summary>
	/// IdentityUser
	/// </summary>	
	public class IdentityUser//可以在这里加上基类等
	{
	//将该表下的字段都遍历出来，可以自定义获取数据描述等信息
    

	  public Guid  Id { get; set; }


	  public string  Name { get; set; }


	  public string  Account { get; set; }


	  public string  Password { get; set; }


	  public Guid  Salt { get; set; }
 
    }
}
	//----------IdentityUser结束----------
	 


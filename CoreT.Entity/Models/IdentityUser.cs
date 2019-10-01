
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
	
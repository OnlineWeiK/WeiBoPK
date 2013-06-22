using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NetDimension.Web;//这个命名空间的代码在App_Code里面
using NetDimension.Weibo;
using System.Configuration;
using System.Text;

public partial class relations : System.Web.UI.Page
{
	Cookie cookie = new Cookie("WeiboDemo", 24, TimeUnit.Hour);

	Client Sina = null;
	string UserID = string.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
		if (string.IsNullOrEmpty(cookie["AccessToken"]))
		{
			Response.RedirectPermanent("Login.aspx");
		}
		else
		{
			Sina = new Client(new OAuth(ConfigurationManager.AppSettings["AppKey"], ConfigurationManager.AppSettings["AppSecret"], cookie["AccessToken"],null)); //用cookie里的accesstoken来实例化OAuth，这样OAuth就有操作权限了
		}

		UserID = Sina.API.Entity.Account.GetUID();

		BindList();
		RelationsTimeline();
    }

	/// <summary>
	/// 获取用户信息，把JSON写到页面的方法和下面的方法区别下
	/// </summary>
	/// <returns>JSON</returns>
	public string LoadUserInfo()
	{
		var user = Sina.API.Dynamic.Users.Show(UserID);
        string test = user["friends_count"];
        //lbl_test.Text = test;
		return string.Format("{0}", user);
	}


	/// <summary>
	/// 数据绑定
	/// </summary>
	public void BindList()
	{

		var json = Sina.API.Dynamic.Suggestions.HotUsers();
		List<object> datasource = new List<object>();
		int i = 0;
		foreach (var user in json)
		{
			var ds = new
			{
				name = user.screen_name,
				desc = user.description.Length >= 20 ? string.Format("{0}", user.description).Substring(0, 20) : user.description,
				pic = user.profile_image_url
			};

			datasource.Add(ds);

			if (++i > 1)	//返回数量
				break;
		}

		rtpFamous.DataSource = datasource;
		rtpFamous.DataBind();

	}

    /// <summary>
    /// 发布一条带图片微博
    /// </summary>
	protected void btnSend_Click(object sender, EventArgs e)
	{
		string status = string.Empty;
		if (txtStatusBody.Text.Length == 0)
		{
            status = "这是一条空微博^_^";
		}
		else
		{
			status = txtStatusBody.Text;
		}

        if (fileUpload1.HasFile)
		{
			dynamic result = Sina.API.Dynamic.Statuses.Upload(status, fileUpload1.FileBytes);
		}
		else
		{
			dynamic result = Sina.API.Dynamic.Statuses.Update(status);
		}

		Response.RedirectPermanent("Default.aspx");
	}

	/// <summary>
	/// 加载互粉好友列表
	/// </summary>
    private void RelationsTimeline()
	{

		StringBuilder relationsBuilder = new StringBuilder();
		string repostPattern = @"	<div class=""status"">
		<a href=""{9}""><img src=""{0}"" alt=""{1}"" url =""{8}"" class=""face"" /></a>
		<p class=""status-cotent""><span class=""name"">{1}</span></p>
		<div class=""repost"">
			<p class=""repost-cotent""><span class=""name"">简介：{2}</p>位置：{3} {8}</span></p>
			<p class=""repost-cotent"">关注 {4} 粉丝 {5} 互粉 {6} 微博 {7} 收藏 {8}
		</div>
	</div>
";

        var json = Sina.API.Dynamic.Friendships.FriendsOnBilateral(UserID);
        if (json.IsDefined("users"))
		{
            //int count = 0;
			foreach (var user in json.users)
			{
                
               			relationsBuilder.AppendFormat(repostPattern,
						user.profile_image_url,
						user.screen_name,
                        user.description,
                        user.location,
                        user.friends_count,//关注数
						user.followers_count,//粉丝数
                        user.bi_followers_count,//互粉数
                        user.statuses_count,//微博数
                        user.favourites_count,//收藏数
                        user.url);  

              
				

			}
		}

		relationsHtml.Text = relationsBuilder.ToString();
	}
   
   
}
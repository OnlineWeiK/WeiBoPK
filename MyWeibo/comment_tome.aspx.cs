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

public partial class comment_tome : System.Web.UI.Page
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
		CommentsTome();
    }

	/// <summary>
	/// 获取用户信息， 直接把JSON写到页面的方法和下面的方法区别下
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
	/// 加载微博列表
	/// </summary>
	private void LoadFriendTimeline()
	{

		StringBuilder statusBuilder = new StringBuilder();
		string imageParttern = @"<img src=""{0}"" alt=""图片"" class=""inner-pic"" />";
		string statusPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-cotent""><span class=""name"">{1}</span>：{2}</p>
		{3}
	</div>
";
		string repostPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-cotent""><span class=""name"">{1}</span>：{2}</p>
		<div class=""repost"">
			<p class=""repost-cotent""><span class=""name"">@{3}</span>：{4}</p>
			{5}
		</div>
	</div>
";

		var json = Sina.API.Dynamic.Statuses.FriendsTimeline(count: 20);
		if (json.IsDefined("statuses"))
		{
			foreach (var status in json.statuses)
			{
				if (!status.IsDefined("user"))
					continue;

				if (status.IsDefined("retweeted_status") && status["retweeted_status"].IsDefined("user"))
				{
					statusBuilder.AppendFormat(repostPattern,
						status.user.profile_image_url,
						status.user.screen_name,
						status.text,
						status.retweeted_status.user.screen_name,
						status.retweeted_status.text,
						status.retweeted_status.IsDefined("thumbnail_pic") ?
						string.Format(imageParttern, status.retweeted_status.thumbnail_pic) : ""  );

				}
				else
				{
					statusBuilder.AppendFormat(statusPattern,
						status.user.profile_image_url,
						status.user.screen_name,
						status.text,
						status.IsDefined("thumbnail_pic") ?
						string.Format(imageParttern, status.thumbnail_pic) : "");
				}

			}
		}

		commentstomeHtml.Text = statusBuilder.ToString();
	}

    private void CommentsTome()
    { 
      	StringBuilder commentsBuilder = new StringBuilder();
		//string imageParttern = @"<img src=""{0}"" alt=""图片"" class=""inner-pic"" />";
		string statusPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-cotent""><span class=""name"">{1}</span>
	</div>
";
		string repostPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-cotent""><span class=""name"">{1}</span></p>关注 ({4}) 粉丝 ({5})
		<div class=""repost"">
			<p class=""repost-cotent"">{2}<span class=""name""></p Time:{3}</span>
			
		</div>
	</div>
";

        var json = Sina.API.Dynamic.Comments.Timeline(count:60);
        if (json.IsDefined("comments"))
		{
            foreach (var comment in json.comments)
			{
				if (!comment.IsDefined("user"))
					continue;

				if (comment.IsDefined("status") && comment["status"].IsDefined("user"))
				{
					commentsBuilder.AppendFormat(repostPattern,
						comment.user.profile_image_url,
						comment.user.screen_name,
						comment.text,
                        comment.created_at,
                        comment.user.friends_count,//关注数
                        comment.user.followers_count);//粉丝数
					//comment.retweeted_status.IsDefined("thumbnail_pic") ?//查找缩略图
					//string.Format(imageParttern, comment.retweeted_status.thumbnail_pic) : "");

				}
				else
				{
                    commentsBuilder.AppendFormat(statusPattern,
                        comment.user.profile_image_url,
                        comment.user.screen_name);
						//comment.text,
						//comment.IsDefined("thumbnail_pic") ?
						//string.Format(imageParttern, comment.thumbnail_pic) : "");
				}

			}
		}

		commentstomeHtml.Text = commentsBuilder.ToString();
    }
    
    protected void Button1_Click(object sender, EventArgs e)
    {
        string userid = Sina.API.Entity.Account.GetUID();
        StringBuilder followsBuilder = new StringBuilder();
        var jason1 = Sina.API.Dynamic.Comments.ToMe(count:20);
        
        // string t[300] = userid["friendships/followers/ids"];

    }
}
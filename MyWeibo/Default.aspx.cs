﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NetDimension.Web;//这个命名空间的代码在App_Code里面
using NetDimension.Weibo;
using System.Configuration;
using System.Text;

public partial class _Default : System.Web.UI.Page
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
		LoadFriendTimeline();
    }

	/// <summary>
	/// 获取用户信息，直接把JSON写到页面的方法和下面的方法区别下
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
    /// 
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
    //protected void btnSend_Click(object sender, EventArgs e)
    //{
    //    string status = string.Empty;
    //    if (txtStatusBody.Text.Length == 0)
    //    {
    //        Page.ClientScript.RegisterStartupScript(GetType(), "MyScript", "<script>alert('请输入微博内容！');location.href='Default.aspx';</script>");
    //        Response.RedirectPermanent("Default.aspx");
    //    }
    //    else
    //    {
    //        status = txtStatusBody.Text;

    //        if (fileUpload1.HasFile && txtStatusBody.Text.Length != 0)
    //        {
    //            dynamic result = Sina.API.Dynamic.Statuses.Upload(status, fileUpload1.FileBytes);
    //            Response.RedirectPermanent("Default.aspx");
    //        }
    //        else
    //        {
    //            dynamic result = Sina.API.Dynamic.Statuses.Update(status);
    //            Response.RedirectPermanent("Default.aspx");
    //        }
    //    }
    //    Response.RedirectPermanent("Default.aspx");
    //}

	/// <summary>
	/// 加载最新微博列表
	/// </summary>
	private void LoadFriendTimeline()
	{

		StringBuilder statusBuilder = new StringBuilder();
		string imageParttern = @"<img src=""{0}"" alt=""图片"" class=""inner-pic"" />";
		string statusPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-cotent""><span class=""name"">{1}</span>：{2}</p>
		{3}</p>评论({4})  转发({5})
	</div>
";
		string repostPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-cotent""><span class=""name"">{1}</span>：{2}</p>
		<div class=""repost"">
			<p class=""repost-cotent""><span class=""name"">@{3}</span>：{4}</p>
			{5}</p>评论({6})  转发({7})
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
						string.Format(imageParttern, status.retweeted_status.thumbnail_pic) : "",
                         status.reposts_count,
                        status.comments_count);

				}
				else
				{
					statusBuilder.AppendFormat(statusPattern,
						status.user.profile_image_url,
						status.user.screen_name,
						status.text,
						status.IsDefined("thumbnail_pic") ?
						string.Format(imageParttern, status.thumbnail_pic) : "",
                         status.reposts_count,
                        status.comments_count);
				}

			}
		}

		statusesHtml.Text = statusBuilder.ToString();
	}
    

   
  
}
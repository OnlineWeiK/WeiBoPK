using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NetDimension.Web;//这个命名空间的代码在App_Code里
using NetDimension.Weibo;
using System.Configuration;
using System.Text;
using System.Windows.Forms;
using System.Threading;


public partial class weibopk : System.Web.UI.Page
{
	Cookie cookie = new Cookie("WeiboDemo", 24, TimeUnit.Hour);
    string[] uid = new string[10000];
	Client Sina = null;
	string UserID = string.Empty;
    int num;
    

    protected void Page_Load(object sender, EventArgs e)
    {
		if (string.IsNullOrEmpty(cookie["AccessToken"]))
		{
			Response.RedirectPermanent("Login.aspx");
		}
        else
        {
            Sina = new Client(new OAuth(ConfigurationManager.AppSettings["AppKey"], ConfigurationManager.AppSettings["AppSecret"], cookie["AccessToken"], null)); //用cookie里的accesstoken来实例化OAuth，这样OAuth就有操作权限了
        }

		UserID = Sina.API.Entity.Account.GetUID();
        Button1.Visible = true;
        myweibopkHtml.Visible = false;
        Image1.Visible = false;
		BindList();
		myweibopk();
    }
    /// <summary>
    /// 定义结构体
    /// </summary>
    public struct Struct
    {
        public Int32 value;
        public string name;
        public string url;

    }
    /// <summary>
    /// 结构体排序遵循的值
    /// </summary>
    public class MyComparer : IComparer<Struct>
    {
        #region IComparer<Struct> 成员

        Int32 IComparer<Struct>.Compare(Struct x, Struct y)
        {
            return y.value - x.value;
        }

        #endregion
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

			if (++i > 1)  //返回数量
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
	/// 微博pk值计算及排序
	/// </summary>
    public void myweibopk()
	{
        StringBuilder pksBuilder = new StringBuilder();
        string repostPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-cotent""><span class=""name"">{1}</span></p>
		<div class=""repost"">
			<p class=""repost-cotent""><span class=""name"">第{2}名</p>PK值：{3}</span></p>
			<p class=""repost-cotent"">
		</div>
	</div>
";
        Struct[] array = new Struct[1000];

        var json = Sina.API.Dynamic.Friendships.Friends(UserID,count:200);//与200个关注好友进行pk    
        if (json.IsDefined("users"))
        {
            int i = 1;
            foreach (var pk in json.users)
            {
                array[i] = new Struct();
                /* PK值计算方程   */
                array[i].value =Convert.ToInt32(0.3 * Convert.ToInt32(pk.friends_count)   +   0.002 * Convert.ToInt32(pk.followers_count) +
                                                0.5 * Convert.ToInt32(pk.bi_followers_count) + 0.7 * Convert.ToInt32(pk.statuses_count) + 0.5 * Convert.ToInt32(pk.favourites_count));
                array[i].name = Convert.ToString(pk.screen_name);
                array[i].url = pk.profile_image_url;
                i++;
             }

            var user = Sina.API.Dynamic.Users.Show(UserID);
            array[i] = new Struct();
            /* PK值计算方程   */
            array[i].value = Convert.ToInt32(0.3*Convert.ToInt32(user["friends_count"]) + 0.002*Convert.ToInt32(user["followers_count"]) +
                                             0.5 * Convert.ToInt32(user["bi_followers_count"]) + 0.7 * Convert.ToInt32(user["statuses_count"]) + 0.5 * Convert.ToInt32(user["favourites_count"]));
            array[i].name = Convert.ToString(user["screen_name"]);
            array[i].url = user["profile_image_url"];
           
            Array.Sort(array, new MyComparer());//按pk值进行排序（结构体排序）
            
            int count = 0;
            foreach (Struct s in array)//找到用户的排名
            {
                count++;
                if (String.Compare(s.name, user["screen_name"]) == 0) break;
            }
            pksBuilder.AppendFormat(repostPattern, array[count-1].url, array[count-1].name, count-1, array[count-1].value);//登录用户自身排名及PK值
            num = count - 1;
            int j=1;
            for (; j<i; j++)
            {
                pksBuilder.AppendFormat(repostPattern, array[j].url, array[j].name, j, array[j].value);
                
            }
        }

		myweibopkHtml.Text = pksBuilder.ToString();
	}

    /// <summary>
    /// 开始PK
    /// </summary>
    protected void Button1_Click(object sender, EventArgs e)
    {
        //Int64 i = 1000000;
        Image1.Visible = true;
        Button1.Visible = false;
        Delay();
        Image1.Visible = false;
        btnSend.Text = "分享给好友";
        txtStatusBody.Text = "#微博达人PK#中我在与关注好友的PK中排第" + Convert.ToString(num) + "名!你也来试一试吧^_^";
         }


    /// <summary>
    /// 延时函数
    /// </summary>
    /// <param name="delayTime">需要延时多少秒</param>
    /// <returns></returns>
    public void Delay()
    {
        //Thread.Sleep(2000);
        myweibopkHtml.Visible = true;
    }
    public static void DoEvents()
    { }
}
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Web.Mvc;
using MyPetShop_v3.Models;
using System.Collections.Specialized;
using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Domain;
using Org.BouncyCastle.Asn1.X509;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Aop.Api.Response;
using Aop.Api.Util;
using System.Web;

namespace MyPetShop_v3.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult GenerateCaptchaImage()
        {
            string captchaText = GenerateRandomCode();
            // 将验证码保存到Session中
            Session["CaptchaImageText"] = captchaText;
            using (MemoryStream memStream = new MemoryStream())
            using (Bitmap captchaImage = GenerateCaptchaBitmap(captchaText))
            {
                captchaImage.Save(memStream, ImageFormat.Png);
                return File(memStream.ToArray(), "image/png");
            }
        }

        // 验证码验证
        private bool ValidateCaptcha(string userInput)
        {
            string expectedCaptcha = Session["CaptchaImageText"] as string;
            return userInput.Equals(expectedCaptcha, StringComparison.OrdinalIgnoreCase);
        }
        private string GenerateRandomCode()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }
        private Bitmap GenerateCaptchaBitmap(string text)
        {
            Bitmap bitmap = new Bitmap(200, 50);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                Font font = new Font("Arial", 20);
                graphics.DrawString(text, font, Brushes.Black, 10, 10);
            }
            return bitmap;
        }
        [HttpPost]
        public ActionResult Login(string name, string password, string phone, string captcha)
        {
            // 验证验证码
            if (!ValidateCaptcha(captcha))
            {
                ModelState.AddModelError("", "验证码不正确。");
                return View();
            }
            if (DB.Login(phone, password, name))
            {
                User user = DB.FindUser(phone);
                Session["LoggedInUser"] = user;
                return RedirectToAction("Index", "Home");
            }
            else
                return View();
        }
        // 注册
        public ActionResult Register()
        {   
            return View();
        }
        //注册登录
        [HttpPost]
        public ActionResult RegisterLogin(string name, string phone, string password, string state, string city, string address)
        {
           
           if(DB.RegisterLogin(name,  phone, password, state,  city, address))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                return RedirectToAction("Register", "Account");
            }
            
        }
        // 我的
        public ActionResult My()
        {
            return View();
        }

        //沙箱环境，请求支付链接的地址           
        const string URL = "https://openapi-sandbox.dl.alipaydev.com/gateway.do";
        //APPID即创建应用后生成,沙箱环境中的AppId
        const string APPID = "9021000130601628";
        //商户私钥
        const string APP_PRIVATE_KEY = "MIIEpAIBAAKCAQEAhrVfB/4qW8UZ4lkC2JszSHlzzW4fSMUiyiQI4SWaQFm23fHIKqzxRFCrqF5U2NkDY/mv8ssVS3l/Z91SDmbwbZucbpe8hxxOJEVTVe4VqXRN2j3xfnfFuHykkYftT7EtOQmSRkYCsSHf7f7WcWjup8kkyUFiY91ZOvrgkMClycqJmMcLdI5dogoQ+Z1nV5NJ2u6dc2ckwuPU4r3nzkpFeur5ZkZmph8BxLMHKWvvpSt3V622wwU1encXnyV/bsgdjBAvsOgwogO+eeRJyKw+g5P0FvnDGxkO1koeUvMDwjOAvjIkaVpBEaY91cg+3dL4y1wpUVLpqmlXGEjR2/xmmQIDAQABAoIBAQCAhP37pDRJhcziNMYQXlIZLTacohna1aoRbTvDgpqeFnnvWkP84y70XHrJkeYlVTZ05b1GSRcyAOLa9z7YWsG78SDYKpBF3212EWYmr5WfLEUymLKUVDUhhFmGN0bkJ0fcCROzVAwxbv40FFQHs3MuRSuBj44NdRsyuDQUJpiF4gHSNxtEjJtcCj9hoV3bWREsEf5jiveotwlQQ6qAHDKhHNdtFVnEeK6AxexNA7sdvTdfFa/RVJrHoVhbmKR26mYA3/KM9mwBRSq5unMPya1/q+biI2vp9vaqH1Y+m/gOZcIJ6hSD0P+AwsaN9rKp8Fn/YiwSz9ixvOjNkNz1Z70BAoGBAL8oGS/zP20/MQhEHglOEqrcLbLQtOfvBzvKaMo66/p97fP0hd8+i1YdxC3x1t8d3kZ/RmEoMIhB42HXCXcf7WHgG/DWrH3YK6OoAn5TJQ9dhC7I9wd6xlrTmH0VBySIxqKXTMULnwG5qiG4bw3WcLB7rOJBNJbQKBnhUupWnt9xAoGBALRnW1GLi2ua/YqIDDrIRK/G73NQMMurs7ZW4ZJDl5oTlTZWt+UIoNVy6tTqDXPmNtK9o5UWOROYBdF0jIshfHALeWcQHDN7TXScv+Yg3PiGdc53tKu8FkTOEu7zrK2I0H8iAcTQpvNhDGZVQUc70+QqdWk4xFdZjFOI6+ubILWpAoGAUKnUDxbfUfNTdoCACD8ow27L3eQSIrkL3WApXgUFJRvUuJgbkbvrwjgnW5fsqxQIgskcYs05xZXAJL5AIUOYwS93uuZrvWLl71ZrTvE490XoDHIDQ+W8JAGcHFQuQm1xHJUp8RBZVboKH9abDkTKIr7dklAfp/BIq7sYaQiKXBECgYEAgBmENnsoHNiJgHzqcjMS0t4n2XHvtC/QBIHc2sc0NywAn/0jx3ZA/lADf/xSYSHve+U86vhEvy0LtJdV1NWKTuVW52ABJm0/qrZDbV2YisCvllRZ7jg4d9L8jsBotTCZHC1BpEekxNz8uQ2AtNw+sZ2UkYrFoGDty8NGAp68s0ECgYBd1LPlRrwwRGgOK9FLaES5uv+icofs1XUXfj5rSQPAFZskkNsORB6iCumAU0Ujffisrdd1tnAFERUCRD8dD5w91dRE5MipsYgzM/ojRHXn/IzZR61sMdt5pFRCNV8U3XsID4S4SnPIFeg//ek91vYgQfUSJ52GZSYC0gl2fnusLA==";
        //参数返回格式，只支持json
        const string FORMAT = "json";
        //支持GBK和UTF-8
        const string CHARSET = "UTF-8";
        //支付宝公钥
        const string ALIPAY_PUBLIC_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkzwMH4ZjubYkf9TVVOg36susIkEgfh+2f9Dee1uHa6eTtxUyrkQ6/lwS+aehiCfofwDvOBpr7tJQKDRdMh0KGTsLIB56eNbW6lGWxXQJhsEWEub5ZplOluG2rhwAvf9Yell8A3je5s8i4l3ZfL3Kzj7tlwydvjy9MS4PlvpmgRoK7rvgrrVgkyXRzpU3Wx9hpOmRY+noYYn1pj41MxvD16TLBufj07PABD2tdHFMWfvuDJ803/3wWk/3K1eQwakLxDKi+4u4YiCiiLOMoAAYCSU2bPqlE4MIrHZKi9TuSOQWMG4JOEevTn9JRYCQqzq3q1UhbTRNZ3kcagV+Wv+tDwIDAQAB";

        [HttpPost]
        public ActionResult Recharge(string money)
        {
            IAopClient client = new DefaultAopClient(URL, APPID, APP_PRIVATE_KEY, FORMAT, "2.0", "RSA2", ALIPAY_PUBLIC_KEY, CHARSET, false);

            //实例化具体API对应的request类,类名称和接口名称对应,当前调用接口名称如：
            AlipayTradePrecreateRequest request = new AlipayTradePrecreateRequest();//创建API对应的request类,请求返回二维码
            AlipayTradePagePayRequest requestPagePay = new AlipayTradePagePayRequest();//请求返回支付宝支付网页
            AlipayTradePagePayModel model = new AlipayTradePagePayModel();
            //主要注意的是这个地方的值

            model.Subject = "充值";
            model.Body = "充值";
            model.TotalAmount = money;
            model.OutTradeNo = DateTime.Now.ToString("yyyyMMddHHmmss"); ;//订单号我们是直接用日期产生
            model.ProductCode = "FAST_INSTANT_TRADE_PAY";
            requestPagePay.SetBizModel(model);
            // 设置同步回调地址
            requestPagePay.SetReturnUrl("http://localhost:8080/Account/RechargeSuccess");
            // 设置异步通知接收地址
            requestPagePay.SetNotifyUrl("http://localhost:8080/Account/Notify");
            // 将业务model载入到request
            request.SetBizModel(model);
            AlipayTradePagePayResponse response = client.pageExecute(requestPagePay, null, "post");

            if (!response.IsError)
            {

                Response.Write(response.Body);
                return View();
            }
            else
            {
                var res = new
                {
                    success = false,
                };
                return Json(res);
            }
        }
        public ActionResult RechargeSuccess()
        {
            try
            {
                SortedDictionary<string, string> sPara = GetRequestGet();

                if (sPara.Count > 0) // Check if there are return parameters
                {
                    bool flag = AlipaySignature.RSACheckV1(sPara, ALIPAY_PUBLIC_KEY, "utf-8", "RSA2", false);

                    if (flag) // Verification successful
                    {


                        if (sPara.TryGetValue("total_amount", out string totalAmount))
                        {
                            string loggedInUserPhone = GetLoggedInUserPhone();

                            User reuser = DB.Recharge(loggedInUserPhone, totalAmount);

                            Session["LoggedInUser"] = reuser;
                            return View();
                        }
                        else
                        {
                            return Content("<script>alert('Payment verification successful, but total_amount not found!');</script>");


                        }

                    }
                    else // Verification failed
                    {
                        return Content("<script>alert('Payment verification failed!');</script>");
                    }
                }
                else
                {
                    return Content("<script>location.href='/Account/My'</script>");
                }
            }
            catch (Exception ex)
            {
                return Content($"<script>alert('An error occurred: {ex.Message}');</script>");
            }
        }

        private SortedDictionary<string, string> GetRequestGet()
        {
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            try
            {
                var queryString = Request.QueryString;
                foreach (string key in queryString.AllKeys)
                {
                    sArray.Add(key, queryString[key]);
                }
            }
            catch (Exception ex)
            {

            }
            return sArray;
        }

        private IDictionary<string, string> GetRequestPost()
        {
            IDictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (int i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }

        [HttpPost]
        public ActionResult Notify()
        {
            // 从 HTTP POST 请求中获取支付宝通知参数
            IDictionary<string, string> parameters = GetRequestPost();

            // 验证通知的签名
            bool isSignatureValid = AlipaySignature.RSACheckV1(parameters, ALIPAY_PUBLIC_KEY, CHARSET, "RSA2", false);

            if (isSignatureValid)
            {
                // 验证通过，处理支付结果
                string tradeStatus = parameters["trade_status"];
                string outTradeNo = parameters["out_trade_no"];

                // 根据支付结果更新订单状态等操作
                if (tradeStatus == "TRADE_SUCCESS")
                {
                    // 支付成功，更新用户余额
                    /* UpdateUserBalance(parameters["buyer_id"], parameters["total_amount"]);*/
                }
                else if (tradeStatus == "TRADE_CLOSED")
                {
                    // 订单已关闭
                    // TODO: 处理订单关闭的代码
                }

                // 返回成功响应
                return Content("success");
            }
            else
            {
                // 签名验证失败，可能是伪造的通知
                return Content("fail");
            }
        }
        private string GetLoggedInUserPhone()
        {
            if (Session["LoggedInUser"] is User loggedInUser)
            {
                return loggedInUser.Phone;
            }
            else
            {
                return null; // 或者返回一个默认值，取决于你的需求
            }
        }
    }
}
using System;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace ncmb_rest_demo
{
    class Program
    {
        // アプリケーションキー
        private static readonly string APPLICATION_KEY = "";
        // クライアントキー
        private static readonly string CLIENT_KEY = "";

        // POSTメソッド
        private static readonly string POST = "POST";
        // APIのエンドポイント
        private static readonly string SIG_API_FQDN = "mb.api.cloud.nifty.com";
        // APIのバージョン
        private static readonly string SIG_API_PATH = "/2013-09-01/classes/";
        // Content-Type: JSON
        private static readonly string JSON = "application/json";

        /// <summary>
        /// C#からNCMBを利用するサンプルプログラム
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // クラス名を指定する
            string className = "SplatoonPlayers";

            // タイムスタンプの取得
            string date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            date = date.Replace(":", "%3A");

            // シグネチャ文字列の作成
            string sig_char = "";
            sig_char += "SignatureMethod=HmacSHA256" + "&";
            sig_char += "SignatureVersion=2" + "&";
            sig_char += "X-NCMB-Application-Key=" + APPLICATION_KEY + "&";
            sig_char += "X-NCMB-Timestamp=" + date;

            // 認証データの作成
            string signature = "";
            signature += POST + "\n";
            signature += SIG_API_FQDN + "\n";
            signature += SIG_API_PATH + className + "\n";
            signature += sig_char;

            // 認証データをBase64に変換
            string signatureHash = GetHMACAsBase64(signature, CLIENT_KEY);

            // URLをエンコード
            Uri url = new Uri("https://" + SIG_API_FQDN + SIG_API_PATH + className);

            // WebRequestインスタンスを初期化
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            // タイムアウトの設定
            req.Timeout = 10000;

            // リクエストに使用するメソッドの設定
            req.Method = POST;

            // Content-Typeの設定
            req.ContentType = JSON;

            // HTTPヘッダーの設定
            req.Headers.Add("X-NCMB-Application-Key", APPLICATION_KEY);
            req.Headers.Add("X-NCMB-Signature", signatureHash);
            req.Headers.Add("X-NCMB-Timestamp", date);
            req.Headers.Add("Access-Control-Allow-Origin", "*");

            // コンテントパラメータを設定
            string _content = "{\"PlayerInfo\":{\"Name\":\"かまめし　やまさん\",\"Udemae\":\"S+\",\"Rank\":\"50\"}}";
            byte[] postDataBytes = Encoding.UTF8.GetBytes(_content);

            // リクエストを書き込む
            Stream stream = null;
            try
            {
                stream = req.GetRequestStream();
                stream.Write(postDataBytes, 0, postDataBytes.Length);
            }
            catch (SystemException cause)
            {
                Console.WriteLine(cause);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            // レスポンスを取得する
            HttpWebResponse httpResponse = null;
            Stream streamResponse = null;
            try
            {
                httpResponse = (HttpWebResponse)req.GetResponse();
                streamResponse = httpResponse.GetResponseStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // レスポンスに対する処理は割愛
        }

        /// <summary>
        /// 認証データをBase64に変換、シグネチャを作成
        /// </summary>
        /// <param name="_stringData">認証データ</param>
        /// <param name="_clientKey">秘密鍵 (クライアントキー)</param>
        /// <returns></returns>
        private static string GetHMACAsBase64(string _stringData, string _clientKey)
        {
            // 署名(シグネチャ)生成  
            byte[] secretKeyBArr = Encoding.UTF8.GetBytes(_clientKey);
            byte[] contentBArr = Encoding.UTF8.GetBytes(_stringData);

            // 秘密鍵と認証データより署名作成               
            HMACSHA256 HMACSHA256 = new HMACSHA256();
            HMACSHA256.Key = secretKeyBArr;
            byte[] final = HMACSHA256.ComputeHash(contentBArr);

            // Base64実行変換
            var result = Convert.ToBase64String(final);
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Instagram_Class;
using Newtonsoft.Json.Linq;

namespace InstagramMultiToolv1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private bool is_logging_in = false;

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private List<InstagramClient> ig_clients = new List<InstagramClient>();
        private string target_id = "";
        private JObject target_info;

        private void PrintErrorToLog(RichTextBox tbox, string msg)
        {
            BeginInvoke(new Action(() =>
            {
                tbox.SelectionColor = Color.Red;
                tbox.AppendText("[HATA] ");
                tbox.SelectionColor = Color.Black;
                tbox.AppendText(msg);
                tbox.AppendText(Environment.NewLine);
            }));
        }
        private void PrintSuccessToLog(RichTextBox tbox, string msg)
        {
            BeginInvoke(new Action(() =>
            {
                tbox.SelectionColor = Color.Green;
                tbox.AppendText("[OK] ");
                tbox.SelectionColor = Color.Black;
                tbox.AppendText(msg);
                tbox.AppendText(Environment.NewLine);

                /// THREADING DE SICIYORSUN HALLET
            }));
        }
        private void PrintStatusToLog(RichTextBox tbox, string msg)
        {
            BeginInvoke(new Action(() =>
            {
                tbox.SelectionColor = Color.Blue;
                tbox.AppendText("[DURUM] ");
                tbox.SelectionColor = Color.Black;
                tbox.AppendText(msg);
                tbox.AppendText(Environment.NewLine);
            }));
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            (sender as TabControl).SelectedTab.Focus();
        }
        private void tabControl1_Enter(object sender, EventArgs e)
        {
            (sender as TabControl).SelectedTab.Focus();
        }
        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as TabControl).SelectedTab.Focus();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.instagram.com/hichigo.exe/");
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/BYFatnn");
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://steamcommunity.com/profiles/76561198051814480");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.turkhackteam.org/members/760892.html");
        }

        private Thread login_t;
        private void button2_Click(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() => { label20.Text = (Convert.ToInt32(label20.Text) + 1).ToString(); }));
            login_t = new Thread(log_in_to_accounts);
            login_t.Start();
        }

        private void log_in_to_accounts()
        {
            if (dataGridView1.Rows.Count < 2)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen kullanıcı giriniz!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }

            is_logging_in = true;

            BeginInvoke(new Action(() =>
            {
                button2.Enabled = false;
                button3.Enabled = true;
                PrintStatusToLog(richTextBox1, "Girişler yapılıyor!");
            }));

            ig_clients.Clear();
            BeginInvoke(new Action(() => { label10.Text = ig_clients.Count.ToString(); }));

            for (int i = 0; i < (dataGridView1.Rows.Count - 1); i++)
            {
                if (is_logging_in == false)
                {
                    return;
                }

                var row = dataGridView1.Rows[i];
                var username = row.Cells[0].Value;
                var password = row.Cells[1].Value;
                var proxy = row.Cells[2].Value;

                if (proxy == null) proxy = "Yok";

                if (username == null || password == null)
                {
                    BeginInvoke(new Action(() =>
                    {
                        new ErrorMessageBox("Lütfen boş kutucuk bırakmayın!", FormStartPosition.CenterParent).ShowDialog();
                        button2.Enabled = true;
                        button3.Enabled = false;
                        is_logging_in = false;
                        PrintErrorToLog(richTextBox1, "Girişler durduruldu!");
                    }));
                    return;
                }

                PrintStatusToLog(richTextBox1, "[" + username + "] " + "Giriş yapılıyor!");
                if (proxy.ToString().ToLower() == "yok" )
                {
                    try
                    {
                        InstagramClient ig_client = new InstagramClient(username.ToString(), password.ToString(), 60);
                        ig_client.GetLoginPage();
                        ig_client.Login();
                        if (is_logging_in == false)
                        {
                            return;
                        }
                        ig_clients.Add(ig_client);
                        BeginInvoke(new Action(() => { label10.Text = ig_clients.Count.ToString(); }));
                        PrintSuccessToLog(richTextBox1, "[" + username + "] " + "Başarıyla giriş yaptı!");
                    }
                    catch (InvalidUsername)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Hatalı kullanıcı adı!");
                    }
                    catch (InvalidPassword)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Hatalı şifre: " + password);
                    }
                    catch (InvalidProxy)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Hatalı proxy: " + password);
                    }
                    catch (LoginFailed)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Giriş başarısız! Kullanıcı adı veya şifre hatalı!");
                    }
                    catch (WebException ex)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Bağlantı hatası: " + ex.Message);
                    }
                    catch (ResponseIsNotOK ex)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Bağlantı hatası: " + ex.Message);
                    } catch (CookieNotFound ex)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Bağlantı hatası: " + ex.Message);
                    }
                    
                }
                else
                {
                    try
                    {
                        InstagramClient ig_client = new InstagramClient(username.ToString(), password.ToString(), proxy.ToString(), 60);
                        ig_client.GetLoginPage();
                        ig_client.Login();
                        if (is_logging_in == false)
                        {
                            return;
                        }
                        ig_clients.Add(ig_client);
                        BeginInvoke(new Action(() => { label10.Text = ig_clients.Count.ToString(); }));
                        PrintSuccessToLog(richTextBox1, "[" + username + "] " + "Başarıyla giriş yaptı!");
                    }
                    catch (InvalidUsername)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Hatalı kullanıcı adı!");
                    }
                    catch (InvalidPassword)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Hatalı şifre: " + password);
                    }
                    catch (CookieNotFound)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Giriş başarısız! Cookieler alınamadı!");
                    }
                    catch (LoginFailed)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Giriş başarısız! Kullanıcı adı veya şifre hatalı!");
                    }
                    catch (WebException ex)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Bağlantı hatası: " + ex.Message);
                    }
                    catch (ResponseIsNotOK ex)
                    {
                        PrintErrorToLog(richTextBox1, "[" + username + "] " + "Bağlantı hatası: " + ex.Message);
                    }
                    catch (TaskCanceledException) { }
                    catch (Exception ex)
                    {
                        PrintErrorToLog(richTextBox1, "\n\n[" + username + "] " + "Bu hatayı yapımcıya iletin!\n" + ex.Message + " " + ex.Source + " " + ex.StackTrace + "\n\n");
                    }
                }
            }
            PrintStatusToLog(richTextBox1, "Girişler tamamlandı!");
            BeginInvoke(new Action(() => {
                button2.Enabled = true;
                button3.Enabled = false;
                is_logging_in = false;
            }));
        }
        private void button3_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button3.Enabled = false;
            is_logging_in = false;
            PrintErrorToLog(richTextBox1, "Girişler durduruldu!");
            if (login_t.IsAlive) login_t.Abort();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "TXT Dosyası |*.txt";  
            file.RestoreDirectory = true;
            file.CheckFileExists = true;
            file.Title = "Yüklenecek kullanıcı dosyası seçiniz..";
            file.Multiselect = false;

            if (file.ShowDialog() == DialogResult.OK)
            {
                string path = file.FileName;
                string file_name = file.SafeFileName;
                PrintStatusToLog(richTextBox1, file_name + " Dosyası yükleniyor!");

                TextReader textReader = File.OpenText(path);
                string line;
                while ((line = textReader.ReadLine()) != null)
                {
                    if (line == "" || line == " ") continue;
                    if (line.Split(' ').Count() < 3) continue;

                    string username = line.Split(' ')[0];
                    string password = line.Split(' ')[1];
                    string proxy = line.Split(' ')[2];

                    dataGridView1.Rows.Add(new string[] { username, password, proxy });
                }
                PrintSuccessToLog(richTextBox1, file_name + " Dosyası yüklendi!");
                textReader.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == " " || textBox1.Text == "")
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen bir kullanıcı adı giriniz!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }
            button4.Enabled = false;

            try
            {
                InstagramClient ig_client = new InstagramClient("hichigo", "hichigo", 60);
                var res = ig_client.GetUserInfo(textBox1.Text);
                var text = ig_client.GetTextFromResponse(res);

                listBox1.Items.Clear();
                listBox2.Items.Clear();

                JObject json = JObject.Parse(text);
                target_info = json;

                target_id = json["graphql"]["user"]["id"].Value<string>();
                textBox2.Text = json["graphql"]["user"]["id"].Value<string>();
                textBox3.Text = json["graphql"]["user"]["biography"].Value<string>();
                textBox3.Text = json["graphql"]["user"]["biography"].Value<string>();
                textBox7.Text = json["graphql"]["user"]["full_name"].Value<string>();
                textBox5.Text = json["graphql"]["user"]["edge_followed_by"]["count"].Value<string>();
                textBox6.Text = json["graphql"]["user"]["edge_follow"]["count"].Value<string>();
                textBox4.Text = json["graphql"]["user"]["edge_owner_to_timeline_media"]["count"].Value<string>();
                pictureBox9.ImageLocation = json["graphql"]["user"]["profile_pic_url_hd"].Value<string>();


                if (textBox4.Text != "0")
                {      
                    int i = 1;
                    foreach (var post in json["graphql"]["user"]["edge_owner_to_timeline_media"]["edges"])
                    {
                        listBox1.Items.Add("Gönderi #" + i.ToString());
                        listBox2.Items.Add("Gönderi #" + i.ToString());
                        i++;
                    }
                }
                
            } catch (ResponseIsNotOK)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Bağlantı hatası gerçekleşti! Kullanıcı adını kontrol edin!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            } catch (WebException)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Bağlantı hatası gerçekleşti!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }
            catch (CookieNotFound)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Bağlantı hatası gerçekleşti! Lütfen tekrar deneyin!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                PrintErrorToLog(richTextBox1, "\n\n[" + username + "] " + "Bu hatayı yapımcıya iletin!\n" + ex.Message + " " + ex.Source + " " + ex.StackTrace + "\n\n");
            }
            button4.Enabled = true;
        }

        private void follow()
        {
            foreach(InstagramClient ig_client in ig_clients)
            {
                string _username = ig_client._username;
                try
                { 
                    var res = ig_client.FollowById(target_id);
                    var json = JObject.Parse(ig_client.GetTextFromResponse(res));

                    if (json["status"].Value<string>() != "ok")
                    {
                        PrintErrorToLog(richTextBox2, "[" + _username + "] " + "Takip edilemedi!");
                    } else
                    {
                        PrintSuccessToLog(richTextBox2, "[" + _username + "] " + "Başarıyla hesabı takip etti!");
                        BeginInvoke(new Action(() => { label20.Text = (Convert.ToInt32(label20.Text) + 1).ToString(); }));
                    }

                } catch (WebException ex)
                {
                    PrintErrorToLog(richTextBox2, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (ResponseIsNotOK ex)
                {
                    PrintErrorToLog(richTextBox2, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (CookieNotFound ex)
                {
                    PrintErrorToLog(richTextBox2, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    PrintErrorToLog(richTextBox1, "\n\n[" + username + "] " + "Bu hatayı yapımcıya iletin!\n" + ex.Message + " " + ex.Source + " " + ex.StackTrace + "\n\n");
                }
            }

            PrintSuccessToLog(richTextBox2, "Bütün takip işlemleri bitti!");
            BeginInvoke(new Action(() =>
            {
                button5.Enabled = false;
                button6.Enabled = true;
            }));
        }
        private Thread follow_t;
        private void button6_Click(object sender, EventArgs e)
        {
            if (ig_clients.Count == 0)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen öncelikle hesaplara giriş yapın!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }

            if (target_id == "")
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen öncelikle bir hedef belirleyin!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }

            follow_t = new Thread(follow);
            follow_t.Start();
            PrintSuccessToLog(richTextBox2, "Takip işlemleri başladı!");

            button6.Enabled = false;
            button5.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            button6.Enabled = true;
            if (follow_t.IsAlive) follow_t.Abort();
            PrintErrorToLog(richTextBox2, "Takip işlemleri durduruldu!");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox16.ImageLocation = target_info["graphql"]["user"]["edge_owner_to_timeline_media"]["edges"][listBox1.SelectedIndex]["node"]["thumbnail_src"].Value<string>();
            textBox8.Text = target_info["graphql"]["user"]["edge_owner_to_timeline_media"]["edges"][listBox1.SelectedIndex]["node"]["id"].Value<string>();
            try
            {
                textBox9.Text = target_info["graphql"]["user"]["edge_owner_to_timeline_media"]["edges"][listBox1.SelectedIndex]["node"]["edge_media_to_caption"]["edges"][0]["node"]["text"].Value<string>();
            } catch {
                textBox9.Text = "Açıklama yok.";
            }
        }

        private Thread like_t;
        private void like()
        {
            for(int i = 0; i < ig_clients.Count; i++)
            {
                var ig_client = ig_clients[i];
                string _username = ig_client._username;
                try
                {
                    var res = ig_client.Like(textBox8.Text);
                    var json = JObject.Parse(ig_client.GetTextFromResponse(res));

                    if (json["status"].Value<string>() != "ok")
                    {
                        PrintErrorToLog(richTextBox3, "[" + _username + "] " + "Beğeni gönderilemedi!");
                    }
                    else
                    {
                        PrintSuccessToLog(richTextBox3, "[" + _username + "] " + "Başarıyla gönderi beğenildi takip etti!");
                    }

                }
                catch (WebException ex)
                {
                    PrintErrorToLog(richTextBox3, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (ResponseIsNotOK ex)
                {
                    PrintErrorToLog(richTextBox3, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (CookieNotFound ex)
                {
                    PrintErrorToLog(richTextBox3, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    PrintErrorToLog(richTextBox1, "\n\n[" + username + "] " + "Bu hatayı yapımcıya iletin!\n" + ex.Message + " " + ex.Source + " " + ex.StackTrace + "\n\n");
                }
            }

            PrintSuccessToLog(richTextBox3, "Bütün beğeni işlemleri bitti!");
            BeginInvoke(new Action(() =>
            {
                button8.Enabled = false;
                button7.Enabled = true;
            }));
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (ig_clients.Count == 0)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen öncelikle hesaplara giriş yapın!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }
            if (target_id == "")
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen öncelikle bir hedef belirleyin!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }
            if (listBox1.SelectedItem == null)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen öncelikle bir gönderi seçin!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }

            like_t = new Thread(like);
            like_t.Start();

            button7.Enabled = false;
            button8.Enabled = true;
            PrintStatusToLog(richTextBox3, "Beğeni işlemi başladı!");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            button8.Enabled = false;
            button7.Enabled = true;
            if (like_t.IsAlive)
            {
                like_t.Abort();
            }
            PrintErrorToLog(richTextBox3, "Beğeni işlemleri durduruldu!");
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox19.ImageLocation = target_info["graphql"]["user"]["edge_owner_to_timeline_media"]["edges"][listBox2.SelectedIndex]["node"]["thumbnail_src"].Value<string>();
            textBox11.Text = target_info["graphql"]["user"]["edge_owner_to_timeline_media"]["edges"][listBox2.SelectedIndex]["node"]["id"].Value<string>();
        }

        private Thread comment_t;
        private void comment()
        {
            foreach (InstagramClient ig_client in ig_clients)
            {
                string _username = ig_client._username;
                try
                {
                    var res = ig_client.Comment(textBox11.Text, textBox10.Text);
                    var json = JObject.Parse(ig_client.GetTextFromResponse(res));

                    if (json["status"].Value<string>() != "ok")
                    {
                        PrintErrorToLog(richTextBox4, "[" + _username + "] " + "Yorum gönderilemedi!");
                    }
                    else
                    {
                        PrintSuccessToLog(richTextBox4, "[" + _username + "] " + "Başarıyla yorum gönderildi!");
                    }

                }
                catch (WebException ex)
                {
                    PrintErrorToLog(richTextBox4, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (ResponseIsNotOK ex)
                {
                    PrintErrorToLog(richTextBox4, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (CookieNotFound ex)
                {
                    PrintErrorToLog(richTextBox4, "[" + _username + "] " + "Bağlantı hatası: " + ex.Message);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    PrintErrorToLog(richTextBox1, "\n\n[" + username + "] " + "Bu hatayı yapımcıya iletin!\n" + ex.Message + " " + ex.Source + " " + ex.StackTrace + "\n\n");
                }
            }

            PrintSuccessToLog(richTextBox4, "Bütün yorum işlemleri bitti!");
            BeginInvoke(new Action(() =>
            {
                button10.Enabled = true;
                button9.Enabled = false;
            }));
        }
        private void button10_Click(object sender, EventArgs e)
        {
            if (ig_clients.Count == 0)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen öncelikle hesaplara giriş yapın!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }
            if (target_id == "")
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen öncelikle bir hedef belirleyin!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }
            if (textBox10.Text == "" || textBox10.Text == " ")
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen gönderilecek bir yorum belirleyin!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }
            if (listBox2.SelectedItem == null)
            {
                BeginInvoke(new Action(() => {
                    new ErrorMessageBox("Lütfen öncelikle bir gönderi seçin!", FormStartPosition.CenterParent).ShowDialog();
                }));
                return;
            }

            button10.Enabled = false;
            button9.Enabled = true;
            PrintStatusToLog(richTextBox4, "Yorum işlemi başladı!");

            comment_t = new Thread(comment);
            comment_t.Start();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                button10.Enabled = true;
                button9.Enabled = false;
            }));
            if (comment_t.IsAlive)
            {
                comment_t.Abort();
            }
            PrintErrorToLog(richTextBox4, "Yorum işlemleri durduruldu!");
        }
    }
}

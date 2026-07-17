using System;
using System.Windows.Forms;

namespace ntfysh_client
{
    public partial class SubscribeDialog : Form
    {
        private readonly ListBox _notificationTopics;
        private readonly string? _originalUnique;
        private readonly bool _isEditMode;
        
        public string TopicId => topicId.Text;
        
        public string ServerUrl => serverUrl.Text;
        
        public string Username => username.Text;
        
        public string Password => password.Text;

        public string Unique => $"{topicId.Text}@{serverUrl.Text}";

        public bool UseWebsockets
        {
            get
            {
                switch (connectionType.Text)
                {
                    case "WebSocket（推荐）":
                        return true;

                    case "HTTP 长轮询（稳健）":
                        return false;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public SubscribeDialog(ListBox notificationTopics)
        {
            _notificationTopics = notificationTopics;
            _isEditMode = false;
            InitializeComponent();
        }

        public SubscribeDialog(ListBox notificationTopics, string unique, string topicId, string serverUrl, string? username, string? password, bool useWebsockets)
        {
            _notificationTopics = notificationTopics;
            _originalUnique = unique;
            _isEditMode = true;
            InitializeComponent();

            this.topicId.Text = topicId;
            this.serverUrl.Text = serverUrl;
            this.username.Text = username ?? string.Empty;
            this.password.Text = password ?? string.Empty;
            connectionType.SelectedIndex = useWebsockets ? 0 : 1;
        }

        private void SubscribeDialog_Load(object sender, EventArgs e)
        {
            if (!_isEditMode) connectionType.SelectedIndex = 0;
        }

        private bool ReparseAddress()
        {
            //Separate schema and address
            string[] parts = serverUrl.Text.Split("://", 2);

            //Validate the basic formatting is correct
            if (parts.Length != 2) return false;

            //Take the schema aside for parsing
            string schema = parts[0].ToLower();

            //Ensure the schema is actually valid
            switch (schema)
            {
                case "http":
                case "https":
                case "ws":
                case "wss":
                    break;

                default:
                    return false;
            }

            //Correct the schema based on connection type if required
            if (UseWebsockets)
            {
                switch (schema)
                {
                    case "http":
                        schema = "ws";
                        break;

                    case "https":
                        schema = "wss";
                        break;

                    case "ws":
                    case "wss":
                        break;
                }
            }
            else
            {
                switch (schema)
                {
                    case "ws":
                        schema = "http";
                        break;

                    case "wss":
                        schema = "https";
                        break;

                    case "http":
                    case "https":
                        break;
                }
            }

            //Reconstruct the address
            string finalAddress = schema + "://" + parts[1];

            //Validate the address
            if (!Uri.IsWellFormedUriString(finalAddress, UriKind.Absolute)) return false;

            //Set the final address and OK it
            serverUrl.Text = finalAddress;

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (topicId.Text.Length < 1)
            {
                MessageBox.Show("请指定主题名称。", "未指定主题名称", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                topicId.Focus();
                return;
            }

            if (serverUrl.Text.Length < 1)
            {
                MessageBox.Show("请指定服务器 URL。默认为 wss://ntfy.sh", "未指定服务器 URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                serverUrl.Focus();
                return;
            }

            if (username.Text.Length > 0 && password.Text.Length < 1)
            {
                MessageBox.Show("请同时指定密码", "未指定密码", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                password.Focus();
                return;
            }

            if (password.Text.Length > 0 && username.Text.Length < 1)
            {
                MessageBox.Show("请同时指定用户名", "未指定用户名", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                username.Focus();
                return;
            }

            if (_notificationTopics.Items.Contains(Unique))
            {
                // 编辑模式下，如果新 Unique 与编辑前相同，跳过重复检查
                if (!_isEditMode || Unique != _originalUnique)
                {
                    MessageBox.Show($"服务器 '{serverUrl.Text}' 上的主题 '{topicId.Text}' 已订阅", "主题已订阅", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.None;
                    username.Focus();
                    return;
                }
            }

            try
            {
                if (!ReparseAddress())
                {
                    MessageBox.Show($"指定的服务器 URL 无效。支持的协议：http:// https:// ws:// wss://", "无效的服务器 URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.None;
                    connectionType.Focus();
                    return;
                }
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show($"选中的连接类型 '{connectionType.Text}' 无效。", "无效的连接类型", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                connectionType.Focus();
                return;
            }



            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void topicId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }
        
        private void serverUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void connectionType_TextChanged(object sender, EventArgs e)
        {
            ReparseAddress();
        }
    }
}

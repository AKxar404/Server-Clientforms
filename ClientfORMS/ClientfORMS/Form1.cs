using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClientForms
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private string clientName;

        public Form1()
        {
            InitializeComponent();
            textBox1.KeyDown += TextBox1_KeyDown; // Añadir evento para manejar Enter
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Puedes dejar este método vacío o agregar funcionalidad si es necesario
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Puedes dejar este método vacío o agregar funcionalidad si es necesario
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowIpDialogAndConnect();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
                e.SuppressKeyPress = true; // Para evitar el "ding" del Enter
            }
        }

        private void ShowIpDialogAndConnect()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Ingrese la IP y el Puerto del Servidor";
                Label labelIp = new Label() { Left = 50, Top = 20, Text = "IP del Servidor" };
                TextBox textBoxIp = new TextBox() { Left = 50, Top = 50, Width = 200 };
                Label labelPort = new Label() { Left = 50, Top = 80, Text = "Puerto del Servidor" };
                TextBox textBoxPort = new TextBox() { Left = 50, Top = 110, Width = 200 };
                Button confirmation = new Button() { Text = "Conectar", Left = 150, Width = 100, Top = 140 };
                confirmation.Click += (sender, e) => { dialog.Close(); };

                dialog.Controls.Add(labelIp);
                dialog.Controls.Add(textBoxIp);
                dialog.Controls.Add(labelPort);
                dialog.Controls.Add(textBoxPort);
                dialog.Controls.Add(confirmation);
                dialog.AcceptButton = confirmation;

                dialog.ShowDialog();

                string serverIp = textBoxIp.Text;
                string serverPort = textBoxPort.Text;
                if (!string.IsNullOrEmpty(serverIp) && int.TryParse(serverPort, out int port))
                {
                    ConnectToServer(serverIp, port);
                }
                else
                {
                    listBox1.Items.Add("IP o puerto del servidor no ingresados correctamente.");
                }
            }
        }

        private void ConnectToServer(string serverIp, int port)
        {
            try
            {
                if (client != null)
                {
                    client.Close();
                }

                client = new TcpClient(serverIp, port);
                listBox1.Items.Add("Conectado al servidor!");
                listBox1.Items.Add("Ingrese su nombre");

                stream = client.GetStream();

                // Envía el nombre del cliente
                clientName = textBox1.Text; // Asegúrate de que textBox1 es el campo para el nombre del cliente
                

                byte[] msg = Encoding.ASCII.GetBytes(clientName);
                stream.Write(msg, 0, msg.Length);

                // Crea un nuevo thread para recibir mensajes del servidor
                Thread receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                button1.Enabled = false;
                textBox1.Enabled = true;
                button2.Enabled = true;
            }
            catch (SocketException ex)
            {
                listBox1.Items.Add("Error al conectar: " + ex.Message);
                MessageBox.Show("Error al conectar con el servidor: " + ex.Message, "Error de conexión");
            }
            catch (Exception ex)
            {
                listBox1.Items.Add("Error inesperado: " + ex.Message);
                MessageBox.Show("Error inesperado: " + ex.Message, "Error");
            }
        }

        private void SendMessage()
        {
            try
            {
                string message = textBox1.Text;
                if (!string.IsNullOrEmpty(message))
                {
                    string timestampedMessage = $"{DateTime.Now:HH:mm:ss} {clientName}: {message}";
                    byte[] msg = Encoding.ASCII.GetBytes(timestampedMessage);
                    stream.Write(msg, 0, msg.Length);
                    listBox1.Items.Add($"Yo: {timestampedMessage}");
                    textBox1.Text = "";
                }
                else
                {
                    MessageBox.Show("El mensaje no puede estar vacío.", "Error de envío");
                }
            }
            catch (InvalidOperationException ex)
            {
                listBox1.Items.Add("Error al enviar mensaje: " + ex.Message);
                MessageBox.Show("Error al enviar mensaje: " + ex.Message, "Error de envío");
            }
            catch (Exception ex)
            {
                listBox1.Items.Add("Error inesperado: " + ex.Message);
                MessageBox.Show("Error inesperado: " + ex.Message, "Error de envío");
            }
        }

        private void ReceiveMessages()
        {
            byte[] bytes = new byte[256];
            string data = null;

            try
            {
                while (true)
                {
                    int i = stream.Read(bytes, 0, bytes.Length);
                    if (i == 0)
                    {
                        listBox1.Invoke((Action)(() => listBox1.Items.Add("Conexión cerrada por el servidor")));
                        break; // Conexión cerrada por el servidor
                    }
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    listBox1.Invoke((Action)(() => listBox1.Items.Add(data)));
                }
            }
            catch (InvalidOperationException ex)
            {
                listBox1.Invoke((Action)(() => listBox1.Items.Add("Error al recibir mensaje: " + ex.Message)));
            }
            catch (Exception ex)
            {
                listBox1.Invoke((Action)(() => listBox1.Items.Add("Error inesperado: " + ex.Message)));
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (client != null)
            {
                client.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}








using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Multi_Screen_App.Controls
{
    /// <summary>
    /// UserControl personalizzato per messaggi di errore/informazione in stile Material Design Dark
    /// </summary>
    public partial class MessageBox_Custom : UserControl
    {
        public MessageBox_Custom()
        {
            InitializeComponent();
        }

        public enum MessageType
        {
            Error,
            Warning,
            Info,
            Success
        }

        public enum MessageBoxButtons
        {
            OK,
            YesNo,
            YesNoCancel
        }

        public enum MessageBoxResult
        {
            None,
            OK,
            Yes,
            No,
            Cancel
        }

        // Proprietà personalizzabili
        public string Title { get; set; } = "Messaggio";
        public string Message { get; set; } = "";
        public MessageType Type { get; set; } = MessageType.Info;
        public MessageBoxButtons Buttons { get; set; } = MessageBoxButtons.OK;
        public double BoxWidth { get; set; } = 400;
        public double BoxHeight { get; set; } = 250;
        public string CustomIcon { get; set; } = null;
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        /// <summary>
        /// Metodo statico per mostrare il MessageBox con solo OK
        /// </summary>
        public static void Show(string message, string title = "Messaggio",
            MessageType type = MessageType.Info, double width = 400, double height = 250,
            string customIcon = null, Window owner = null)
        {
            ShowDialog(message, title, type, MessageBoxButtons.OK, width, height, customIcon, owner);
        }

        /// <summary>
        /// Metodo statico per mostrare il MessageBox e ottenere il risultato
        /// </summary>
        public static MessageBoxResult ShowDialog(string message, string title = "Messaggio",
            MessageType type = MessageType.Info, MessageBoxButtons buttons = MessageBoxButtons.OK,
            double width = 400, double height = 250, string customIcon = null, Window owner = null)
        {
            var window = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                AllowsTransparency = true,
                ShowInTaskbar = false,
                Owner = owner
            };

            if (owner != null)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            var msgBox = new MessageBox_Custom
            {
                Title = title,
                Message = message,
                Type = type,
                Buttons = buttons,
                BoxWidth = width,
                BoxHeight = height,
                CustomIcon = customIcon
            };

            msgBox.BuildUI(window);
            window.Content = msgBox;
            window.ShowDialog();

            return msgBox.Result;
        }

        /// <summary>
        /// Costruisce l'interfaccia del MessageBox
        /// </summary>
        public void BuildUI(Window parentWindow)
        {
            // Grid principale
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });

            // Barra del titolo
            var titleBar = CreateTitleBar(parentWindow);
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Area messaggio
            var messageArea = CreateMessageArea();
            Grid.SetRow(messageArea, 1);
            mainGrid.Children.Add(messageArea);

            // Area pulsanti
            var buttonArea = CreateButtonArea(parentWindow);
            Grid.SetRow(buttonArea, 2);
            mainGrid.Children.Add(buttonArea);

            this.Content = mainGrid;
        }

        /// <summary>
        /// Crea la barra del titolo con icona e testo
        /// </summary>
        private Border CreateTitleBar(Window parentWindow)
        {
            var titleBorder = new Border
            {
                Background = GetColorByType(),
                CornerRadius = new CornerRadius(8, 8, 0, 0)
            };

            var titleGrid = new Grid();
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            // Icona
            var iconText = new TextBlock
            {
                Text = GetIconByType(),
                FontSize = 24,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI Symbol")
            };
            Grid.SetColumn(iconText, 0);
            titleGrid.Children.Add(iconText);

            // Titolo
            var titleText = new TextBlock
            {
                Text = Title,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("#3ds Light"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(titleText, 1);
            titleGrid.Children.Add(titleText);

            // Pulsante chiudi
            var closeButton = new Button
            {
                Content = "✕",
                FontSize = 18,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontFamily = new FontFamily("Segoe UI Symbol"),
                Style = CreateHoverButtonStyle()
            };
            closeButton.Click += (s, e) =>
            {
                Result = MessageBoxResult.None;
                parentWindow.Close();
            };
            Grid.SetColumn(closeButton, 2);
            titleGrid.Children.Add(closeButton);

            titleBorder.Child = titleGrid;
            return titleBorder;
        }

        /// <summary>
        /// Crea l'area del messaggio
        /// </summary>
        private Border CreateMessageArea()
        {
            var messageBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                Padding = new Thickness(20)
            };

            var messageScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            var messageText = new TextBlock
            {
                Text = Message,
                FontSize = 14,
                FontFamily = new FontFamily("#3ds Light"),
                Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            };

            messageScroll.Content = messageText;
            messageBorder.Child = messageScroll;
            return messageBorder;
        }

        /// <summary>
        /// Crea l'area dei pulsanti in base al tipo
        /// </summary>
        private Border CreateButtonArea(Window parentWindow)
        {
            var buttonBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
                CornerRadius = new CornerRadius(0, 0, 8, 8),
                Padding = new Thickness(20, 10, 20, 10)
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            switch (Buttons)
            {
                case MessageBoxButtons.OK:
                    buttonPanel.Children.Add(CreateButton("OK", MessageBoxResult.OK, parentWindow, false));
                    break;

                case MessageBoxButtons.YesNo:
                    buttonPanel.Children.Add(CreateButton("No", MessageBoxResult.No, parentWindow, false));
                    buttonPanel.Children.Add(CreateButton("Yes", MessageBoxResult.Yes, parentWindow, false));
                    break;

                case MessageBoxButtons.YesNoCancel:
                    buttonPanel.Children.Add(CreateButton("Cancel", MessageBoxResult.Cancel, parentWindow, false));
                    buttonPanel.Children.Add(CreateButton("No", MessageBoxResult.No, parentWindow, false));
                    buttonPanel.Children.Add(CreateButton("Yes", MessageBoxResult.Yes, parentWindow, false));
                    break;
            }

            buttonBorder.Child = buttonPanel;
            return buttonBorder;
        }

        /// <summary>
        /// Crea un singolo pulsante
        /// </summary>
        private Button CreateButton(string text, MessageBoxResult result, Window parentWindow, bool isPrimary)
        {
            var button = new Button
            {
                Content = text,
                Width = 100,
                Height = 35,
                Background = isPrimary ? GetColorByType() : new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontFamily = new FontFamily("#3ds Light"),
                FontWeight = FontWeights.Medium,
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(5, 0, 0, 0)
            };

            button.Click += (s, e) =>
            {
                Result = result;
                parentWindow.Close();
            };

            // Effetto hover
            button.MouseEnter += (s, e) =>
            {
                if (isPrimary)
                {
                    button.Opacity = 0.8;
                }
                else
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(90, 90, 90));
                }
            };

            button.MouseLeave += (s, e) =>
            {
                if (isPrimary)
                {
                    button.Opacity = 1.0;
                }
                else
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                }
            };

            return button;
        }

        /// <summary>
        /// Crea uno style per l'hover del pulsante
        /// </summary>
        private Style CreateHoverButtonStyle()
        {
            var style = new Style(typeof(Button));
            var template = new ControlTemplate(typeof(Button));

            return style;
        }

        /// <summary>
        /// Restituisce il colore in base al tipo di messaggio
        /// </summary>
        private Brush GetColorByType()
        {
            switch (Type)
            {
                case MessageType.Error:
                    return new SolidColorBrush(Color.FromRgb(211, 47, 47));
                case MessageType.Warning:
                    return new SolidColorBrush(Color.FromRgb(18, 184, 255)); //ori new SolidColorBrush(Color.FromRgb(245, 124, 0));  
                case MessageType.Success:
                    return new SolidColorBrush(Color.FromRgb(56, 142, 60));
                case MessageType.Info:
                default:
                    return new SolidColorBrush(Color.FromRgb(25, 118, 210));
            }
        }

        /// <summary>
        /// Restituisce l'icona in base al tipo di messaggio
        /// </summary>
        private string GetIconByType()
        {
            if (!string.IsNullOrEmpty(CustomIcon))
                return CustomIcon;

            switch (Type)
            {
                case MessageType.Error:
                    return "⚠";
                case MessageType.Warning:
                    return "⚡";
                case MessageType.Success:
                    return "✓";
                case MessageType.Info:
                default:
                    return "ℹ";
            }
        }
    }
}

// ===== ESEMPI DI UTILIZZO =====
/*

using TelemetryLab.Controls;

// ========== MESSAGGI CON SOLO OK ==========

// Messaggio di errore (solo OK)
MessageBox_Custom.Show(
    "Si è verificato un errore durante l'operazione.",
    "Errore",
    MessageBox_Custom.MessageType.Error
);

// Messaggio di successo (solo OK)
MessageBox_Custom.Show(
    "Operazione completata con successo!",
    "Successo",
    MessageBox_Custom.MessageType.Success
);


// ========== MESSAGGI CON YES/NO ==========

// Conferma eliminazione
var result = MessageBox_Custom.ShowDialog(
    "Sei sicuro di voler eliminare questo elemento?",
    "Conferma eliminazione",
    MessageBox_Custom.MessageType.Warning,
    MessageBox_Custom.MessageBoxButtons.YesNo
);

if (result == MessageBox_Custom.MessageBoxResult.Yes)
{
    // Utente ha cliccato Sì
    // Esegui l'eliminazione
}
else if (result == MessageBox_Custom.MessageBoxResult.No)
{
    // Utente ha cliccato No
    // Annulla operazione
}


// ========== MESSAGGI CON YES/NO/CANCEL ==========

// Salvataggio modifiche
var saveResult = MessageBox_Custom.ShowDialog(
    "Vuoi salvare le modifiche prima di uscire?",
    "Salva modifiche",
    MessageBox_Custom.MessageType.Info,
    MessageBox_Custom.MessageBoxButtons.YesNoCancel,
    450,
    200
);

switch (saveResult)
{
    case MessageBox_Custom.MessageBoxResult.Yes:
        // Salva e esci
        SaveChanges();
        CloseWindow();
        break;
    
    case MessageBox_Custom.MessageBoxResult.No:
        // Esci senza salvare
        CloseWindow();
        break;
    
    case MessageBox_Custom.MessageBoxResult.Cancel:
    case MessageBox_Custom.MessageBoxResult.None:
        // Rimani nella finestra
        break;
}


// ========== ESEMPIO CON TRY-CATCH ==========

try
{
    // Il tuo codice che può generare eccezioni
    DeleteFile("important.txt");
}
catch (Exception ex)
{
    var result = MessageBox_Custom.ShowDialog(
        $"Errore durante l'eliminazione del file:\n{ex.Message}\n\nVuoi riprovare?",
        "Errore",
        MessageBox_Custom.MessageType.Error,
        MessageBox_Custom.MessageBoxButtons.YesNo,
        500,
        300
    );

    if (result == MessageBox_Custom.MessageBoxResult.Yes)
    {
        // Riprova l'operazione
        RetryOperation();
    }
}


// ========== ESEMPIO PRATICO: CHIUSURA APPLICAZIONE ==========

private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
{
    var result = MessageBox_Custom.ShowDialog(
        "Sei sicuro di voler uscire dall'applicazione?",
        "Conferma uscita",
        MessageBox_Custom.MessageType.Warning,
        MessageBox_Custom.MessageBoxButtons.YesNo,
        owner: this
    );

    if (result != MessageBox_Custom.MessageBoxResult.Yes)
    {
        e.Cancel = true; // Annulla la chiusura
    }
}


// ========== ESEMPIO: SOVRASCRITTURA FILE ==========

private void SaveFile(string path)
{
    if (File.Exists(path))
    {
        var result = MessageBox_Custom.ShowDialog(
            "Il file esiste già. Vuoi sovrascriverlo?",
            "File esistente",
            MessageBox_Custom.MessageType.Warning,
            MessageBox_Custom.MessageBoxButtons.YesNo
        );

        if (result != MessageBox_Custom.MessageBoxResult.Yes)
        {
            return; // Non salvare
        }
    }

    // Procedi con il salvataggio
    File.WriteAllText(path, content);
    
    MessageBox_Custom.Show(
        "File salvato con successo!",
        "Successo",
        MessageBox_Custom.MessageType.Success
    );
}

*/
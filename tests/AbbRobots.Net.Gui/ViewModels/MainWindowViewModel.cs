using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using AbbRobots.Net.Models;
using AbbRobots.Net.Parser;
using ReactiveUI;

namespace AbbRobots.Net.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _statusMessage = "Esperando archivo EIO...";
    public ObservableCollection<SignalItem> Signals { get; set; } = new();

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    // Botón de la pantalla
    public ICommand CargarArchivoCommand { get; }
    
    public MainWindowViewModel()
    {
        CargarArchivoCommand = ReactiveCommand.Create(CargarEio);
    }

    private void CargarEio()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "EIO.cfg");
        try
        {
            if (!File.Exists(filePath))
            {
                StatusMessage = "File not found!";
                return;
            }
    
            string content = File.ReadAllText(filePath);
            
            // 💡 SOLUCIÓN 2: Creamos el parser y procesamos el texto del archivo
            var parser = new EioParser();
            parser.ProcessEioFile(content);

            // 💡 SOLUCIÓN 1: Añadidos los paréntesis () a Clear
            Signals.Clear(); 
            
            foreach (var signal in parser.Signals)
            {
                Signals.Add(signal);
            }
            
            StatusMessage = $"¡Éxito! Se han cargado {Signals.Count} señales del robot.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al procesar: {ex.Message}";
        }
    }
};
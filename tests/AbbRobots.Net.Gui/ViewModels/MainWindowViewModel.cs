using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AbbRobots.Net.Models;
using AbbRobots.Net.Parsers;
using ReactiveUI;

namespace AbbRobots.Net.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string _statusMessage="Esperando archivo EIO...";
    public ObservableCollection <SignalItem> Signals {get; set;}= new();

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    //Botón de la pantalla
    public ICommand CargarArchivoCommand{get;}
    public MainWindowViewModel()
    {
        CargarArchivoCommand = ReactiveCommand.Create(CargarEio);
    }

    private void CargarEio()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory,"EIO.cfg");
        try
        {
            if (!File.Exist(filePath))
            {
                StatusMessage = $"File not found!";
                return;
            }
    
            string content =File.ReadAllText(filePath);
            Signals.Clear;
            foreach (var signal parser.Signals)
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
}

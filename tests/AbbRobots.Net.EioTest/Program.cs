using System;
using System.IO;
using System.Collections.Generic;
// Asegúrate de cambiar este using por el namespace exacto donde viva tu EioParser
using AbbRobots.Net.Parser; 

Console.WriteLine("=== TEST DE PURGADO DE EIO.CFG ===");

string rutaEio = "EIO.cfg"; // Asegúrate de copiar un EIO.cfg de prueba en la carpeta del ejecutable

if (!File.Exists(rutaEio))
{
    Console.WriteLine($"[Error] No se encuentra el archivo en: {Path.GetFullPath(rutaEio)}");
    return;
}

// 1. Cargamos el documento usando tu parser existente
Console.WriteLine("Cargando y parseando archivo...");
var Eio = new EioParser[];
 Eio.ProcessEioFile(rutaEio);

// 2. Obtenemos la lista de señales mapeadas por el parser
var señalesEio = eioDocument.GetSignals();
Console.WriteLine($"Señales iniciales encontradas: {señalesEio.Count}\n");

// 3. Bucle inverso de toda la vida para borrar elementos de una lista de forma segura
for (int i = señalesEio.Count - 1; i >= 0; i--)
{
    var señal = señalesEio[i];
    
    // ¡Aquí obtenemos el nombre gracias a tu parser!
    string nombreSeñal = señal.Name;

    // --- FILTRO POR NUEVA CARACTERÍSTICA ---
    // Aquí es donde decides si la señal se queda o se va
    if (DebeBorrarsePorNuevaCaracteristica(señal))
    {
        Console.WriteLine($"[BORRANDO] -> {nombreSeñal} (Tipo: {señal.SignalType})");
        
        // Eliminamos la señal de la lista interna del documento
        señalesEio.RemoveAt(i);
    }
}

// 4. Guardamos el resultado limpio
string rutaSalida = "EIO_Limpio.cfg";
eioDocument.Save(rutaSalida);

Console.WriteLine($"\nProceso terminado. Archivo purgado guardado en: {Path.GetFullPath(rutaSalida)}");



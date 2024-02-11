﻿using MathNet.Spatial.Euclidean;
using MechanicsCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MechanicsUI;

public class RenderVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public SimulationVM SimulationVM { get; }
    public Perspective Perspective { get; }
    public ObservableCollection<BodyVM> BodyVMs { get; }
    public double CanvasTranslateX { get; private set; }
    public double CanvasTranslateY { get; private set; }
    public double CanvasScale { get; private set; } = 1;

    private Size _availableSizePix;
    public Size AvailableSizePix
    {
        set
        {
            _availableSizePix = value;
            RefreshBounds();
        }
    }

    public RenderVM(SimulationVM simulationVM, Perspective perspective)
    {
        SimulationVM = simulationVM;
        Perspective = perspective;
        BodyVMs = new(Simulation.Bodies.Select(b => new BodyVM(b, this)));
    }

    private Simulation Simulation => SimulationVM.Model;
    public Vector3D PanelDisplayBound0 => Perspective.SimToPanel(Simulation.DisplayBound0);
    public Vector3D PanelDisplayBound1 => Perspective.SimToPanel(Simulation.DisplayBound1);
    public double GridWidth => Math.Abs(PanelDisplayBound1.X - PanelDisplayBound0.X);
    public double GridHeight => Math.Abs(PanelDisplayBound1.Y - PanelDisplayBound0.Y);

    private static readonly PropertyChangedEventArgs sCanvasTranslateXChangedArgs = new(nameof(CanvasTranslateX));
    private static readonly PropertyChangedEventArgs sCanvasTranslateYChangedArgs = new(nameof(CanvasTranslateY));
    private static readonly PropertyChangedEventArgs sCanvasScaleChangedArgs = new(nameof(CanvasScale));

    internal void RefreshSim()
    {
        for (var i = BodyVMs.Count - 1; i >= 0; i--)
        {
            var bodyVM = BodyVMs[i];
            if (bodyVM.Model.Exists)
            {
                bodyVM.Refresh();
            }
            else
            {
                BodyVMs.RemoveAt(i);
            }
        }
    }

    private void RefreshBounds()
    {
        var panelBound0 = PanelDisplayBound0;
        var panelBound1 = PanelDisplayBound1;
        Sort(panelBound0.X, panelBound1.X, out var xMin, out var xMax);
        Sort(panelBound0.Y, panelBound1.Y, out var yMin, out var yMax);
        var systemWidth = xMax - xMin;
        var systemHeight = yMax - yMin;
        var xScale = _availableSizePix.Width / systemWidth;
        var yScale = _availableSizePix.Height / systemHeight;
        CanvasScale = Math.Min(xScale, yScale);
        CanvasTranslateX = -(xMin + xMax) / 2 * xScale;
        CanvasTranslateY = -(yMin + yMax) / 2 * yScale;
        PropertyChanged?.Invoke(this, sCanvasTranslateXChangedArgs);
        PropertyChanged?.Invoke(this, sCanvasTranslateYChangedArgs);
        PropertyChanged?.Invoke(this, sCanvasScaleChangedArgs);
    }

    public static void Sort(double a, double b, out double min, out double max)
    {
        if (a > b)
        {
            min = b;
            max = a;
        }
        else
        {
            min = a;
            max = b;
        }
    }
}

public class DesignRenderVM : RenderVM
{
    public DesignRenderVM()
        : base(new DefaultSimulationVM(), Perspective.Orthogonal_FromAbove)
    {
    }
}

﻿using MathNet.Spatial.Euclidean;
using MechanicsCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MechanicsUI;

public class RenderVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

    public SimulationVM SimulationVM { get; }
    public Perspective Perspective { get; }
    public ObservableCollection<BodyVM> BodyVMs { get; }
    private readonly Dictionary<Body, BodyVM> _bodyVMsByModel;
    public ObservableCollectionPlus<BodyVM> BodyVMsByDistance { get; set; } = new();

    private static readonly PropertyChangedEventArgs sCanvasTranslateXChangedArgs = new(nameof(CanvasTranslateX));
    public double CanvasTranslateX { get; private set; }

    private static readonly PropertyChangedEventArgs sCanvasTranslateYChangedArgs = new(nameof(CanvasTranslateY));
    public double CanvasTranslateY { get; private set; }

    private static readonly PropertyChangedEventArgs sCanvasScaleChangedArgs = new(nameof(CanvasScale));
    public double CanvasScale { get; private set; } = 1;

    private Point? _mousePosition;
    private Size _availableSizePix;

    public RenderVM(SimulationVM simulationVM, Perspective perspective)
    {
        SimulationVM = simulationVM;
        Perspective = perspective;
        BodyVMs = new(Simulation.Bodies.Select(b => new BodyVM(b, this)));
        _bodyVMsByModel = BodyVMs.ToDictionary(b => b.Model);
        RefreshSim();

        SimulationVM.PropertyChanged += SimulationVM_PropertyChanged;
    }

    public void Unhook()
    {
        SimulationVM.PropertyChanged -= SimulationVM_PropertyChanged;
        foreach (var bodyVM in BodyVMs)
            bodyVM.Unhook();
    }

    private void SimulationVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SimulationVM.MinGlowRadius):
            case nameof(SimulationVM.GlowPush):
                // We used to only refresh the radii when the glow radius changed,
                // but now the glow radius can affect the position,
                // so let's just refresh everything.
                RefreshSim();
                break;
        }
    }

    private Simulation Simulation => SimulationVM.Model;
    public Vector3D PanelDisplayBound0 => Perspective.SimToPanel(Simulation.DisplayBound0);
    public Vector3D PanelDisplayBound1 => Perspective.SimToPanel(Simulation.DisplayBound1);
    public double GridWidth => Math.Abs(PanelDisplayBound1.X - PanelDisplayBound0.X);
    public double GridHeight => Math.Abs(PanelDisplayBound1.Y - PanelDisplayBound0.Y);

    public Size AvailableSizePix
    {
        set
        {
            if (_availableSizePix == value) return;
            _availableSizePix = value;
            RefreshBounds();
        }
    }

    internal void RefreshSim()
    {
        for (var i = BodyVMs.Count - 1; i >= 0; i--)
        {
            var bodyVM = BodyVMs[i];
            if (bodyVM.Model.Exists)
            {
                bodyVM.Refresh_1_of_2();
            }
            else
            {
                BodyVMs.RemoveAt(i);
                _bodyVMsByModel.Remove(bodyVM.Model);
                bodyVM.Unhook();
            }
        }
        foreach (var bodyVM in BodyVMs)
        {
            bodyVM.Refresh_2_of_2(b => _bodyVMsByModel[b].GlowRadius);
        }
        RefreshByDistance(_mousePosition);
    }

    public void RefreshByDistance(Point? mousePosition)
    {
        _mousePosition = mousePosition;
        if (mousePosition == null)
            return;
        var byDistance = BodyVMs
            .OrderBy(b =>
            {
                var dx = b.PanelCenterXY.X - mousePosition.Value.X;
                var dy = b.PanelCenterXY.Y - mousePosition.Value.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            })
            .Take(10)
            .ToList();
        if (!BodyVMsByDistance.SequenceEqual(byDistance))
            BodyVMsByDistance.Reset(byDistance);
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
        OnPropertyChanged(sCanvasTranslateXChangedArgs);
        OnPropertyChanged(sCanvasTranslateYChangedArgs);
        OnPropertyChanged(sCanvasScaleChangedArgs);
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

namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Spatial;

    internal class SpatialValidatorImplementation : SpatialPipeline
    {
        private readonly NestedValidator geographyValidatorInstance = new NestedValidator();
        private readonly NestedValidator geometryValidatorInstance = new NestedValidator();
        internal const double MaxLatitude = 90.0;
        internal const double MaxLongitude = 15069.0;

        public override System.Spatial.GeographyPipeline GeographyPipeline
        {
            get
            {
                return this.geographyValidatorInstance.GeographyPipeline;
            }
        }

        public override System.Spatial.GeometryPipeline GeometryPipeline
        {
            get
            {
                return this.geometryValidatorInstance.GeometryPipeline;
            }
        }

        private class NestedValidator : DrawBoth
        {
            private static readonly ValidatorState BeginSpatial = new BeginGeoState();
            private static readonly ValidatorState Collection = new CollectionState();
            private static readonly ValidatorState CoordinateSystem = new SetCoordinateSystemState();
            private int depth;
            private static readonly ValidatorState FullGlobe = new FullGlobeState();
            private double initialFirstCoordinate;
            private double initialSecondCoordinate;
            private static readonly ValidatorState LineStringBuilding = new LineStringBuildingState();
            private static readonly ValidatorState LineStringEnd = new LineStringEndState();
            private static readonly ValidatorState LineStringStart = new LineStringStartState();
            private const int MaxGeometryCollectionDepth = 0x1c;
            private double mostRecentFirstCoordinate;
            private double mostRecentSecondCoordinate;
            private static readonly ValidatorState MultiLineString = new MultiLineStringState();
            private static readonly ValidatorState MultiPoint = new MultiPointState();
            private static readonly ValidatorState MultiPolygon = new MultiPolygonState();
            private static readonly ValidatorState PointBuilding = new PointBuildingState();
            private int pointCount;
            private static readonly ValidatorState PointEnd = new PointEndState();
            private static readonly ValidatorState PointStart = new PointStartState();
            private static readonly ValidatorState PolygonBuilding = new PolygonBuildingState();
            private static readonly ValidatorState PolygonStart = new PolygonStartState();
            private bool processingGeography;
            private int ringCount;
            private readonly Stack<ValidatorState> stack = new Stack<ValidatorState>(0x10);
            private System.Spatial.CoordinateSystem validationCoordinateSystem;

            public NestedValidator()
            {
                this.InitializeObject();
            }

            private void AddControlPoint(double first, double second)
            {
                this.Execute(PipelineCall.LineTo);
                this.TrackPosition(first, second);
            }

            private static bool AreLongitudesEqual(double left, double right)
            {
                if (left != right)
                {
                    return (((left - right) % 360.0) == 0.0);
                }
                return true;
            }

            private void BeginFigure(Action<double, double, double?, double?> validate, double x, double y, double? z, double? m)
            {
                validate(x, y, z, m);
                this.Execute(PipelineCall.BeginFigure);
                this.pointCount = 0;
                this.TrackPosition(x, y);
            }

            private void BeginShape(SpatialType type)
            {
                this.depth++;
                switch (type)
                {
                    case SpatialType.Point:
                        this.Execute(PipelineCall.BeginPoint);
                        return;

                    case SpatialType.LineString:
                        this.Execute(PipelineCall.BeginLineString);
                        return;

                    case SpatialType.Polygon:
                        this.Execute(PipelineCall.BeginPolygon);
                        return;

                    case SpatialType.MultiPoint:
                        this.Execute(PipelineCall.BeginMultiPoint);
                        return;

                    case SpatialType.MultiLineString:
                        this.Execute(PipelineCall.BeginMultiLineString);
                        return;

                    case SpatialType.MultiPolygon:
                        this.Execute(PipelineCall.BeginMultiPolygon);
                        return;

                    case SpatialType.Collection:
                        this.Execute(PipelineCall.BeginCollection);
                        return;

                    case SpatialType.FullGlobe:
                        if (!this.processingGeography)
                        {
                            throw new FormatException(Strings.Validator_InvalidType(type));
                        }
                        this.Execute(PipelineCall.BeginFullGlobe);
                        return;
                }
                throw new FormatException(Strings.Validator_InvalidType(type));
            }

            private void Call(ValidatorState state)
            {
                if (this.stack.Count > 0x1c)
                {
                    throw new FormatException(Strings.Validator_NestingOverflow(0x1c));
                }
                this.stack.Push(state);
            }

            private void Execute(PipelineCall transition)
            {
                this.stack.Peek().ValidateTransition(transition, this);
            }

            private void InitializeObject()
            {
                this.depth = 0;
                this.initialFirstCoordinate = 0.0;
                this.initialSecondCoordinate = 0.0;
                this.mostRecentFirstCoordinate = 0.0;
                this.mostRecentSecondCoordinate = 0.0;
                this.pointCount = 0;
                this.validationCoordinateSystem = null;
                this.ringCount = 0;
                this.stack.Clear();
                this.stack.Push(CoordinateSystem);
            }

            private static bool IsFinite(double value)
            {
                return (!double.IsNaN(value) && !double.IsInfinity(value));
            }

            private static bool IsLatitudeValid(double latitude)
            {
                return ((latitude >= -90.0) && (latitude <= 90.0));
            }

            private static bool IsLongitudeValid(double longitude)
            {
                return ((longitude >= -15069.0) && (longitude <= 15069.0));
            }

            private static bool IsPointValid(double first, double second, double? z, double? m)
            {
                if ((!IsFinite(first) || !IsFinite(second)) || (z.HasValue && !IsFinite(z.Value)))
                {
                    return false;
                }
                if (m.HasValue)
                {
                    return IsFinite(m.Value);
                }
                return true;
            }

            private void Jump(ValidatorState state)
            {
                this.stack.Pop();
                this.stack.Push(state);
            }

            protected override GeographyPosition OnBeginFigure(GeographyPosition position)
            {
                this.BeginFigure(new Action<double, double, double?, double?>(SpatialValidatorImplementation.NestedValidator.ValidateGeographyPosition), position.Latitude, position.Longitude, position.Z, position.M);
                return position;
            }

            protected override GeometryPosition OnBeginFigure(GeometryPosition position)
            {
                this.BeginFigure(new Action<double, double, double?, double?>(SpatialValidatorImplementation.NestedValidator.ValidateGeometryPosition), position.X, position.Y, position.Z, position.M);
                return position;
            }

            protected override SpatialType OnBeginGeography(SpatialType shape)
            {
                if ((this.depth > 0) && !this.processingGeography)
                {
                    throw new FormatException(Strings.Validator_UnexpectedGeometry);
                }
                this.processingGeography = true;
                this.BeginShape(shape);
                return shape;
            }

            protected override SpatialType OnBeginGeometry(SpatialType shape)
            {
                if ((this.depth > 0) && this.processingGeography)
                {
                    throw new FormatException(Strings.Validator_UnexpectedGeography);
                }
                this.processingGeography = false;
                this.BeginShape(shape);
                return shape;
            }

            protected override void OnEndFigure()
            {
                this.Execute(PipelineCall.EndFigure);
            }

            protected override void OnEndGeography()
            {
                this.Execute(PipelineCall.End);
                if (!this.processingGeography)
                {
                    throw new FormatException(Strings.Validator_UnexpectedGeometry);
                }
                this.depth--;
            }

            protected override void OnEndGeometry()
            {
                this.Execute(PipelineCall.End);
                if (this.processingGeography)
                {
                    throw new FormatException(Strings.Validator_UnexpectedGeography);
                }
                this.depth--;
            }

            protected override GeographyPosition OnLineTo(GeographyPosition position)
            {
                if (this.processingGeography)
                {
                    ValidateGeographyPosition(position.Latitude, position.Longitude, position.Z, position.M);
                }
                this.AddControlPoint(position.Latitude, position.Longitude);
                if (!this.processingGeography)
                {
                    throw new FormatException(Strings.Validator_UnexpectedGeometry);
                }
                return position;
            }

            protected override GeometryPosition OnLineTo(GeometryPosition position)
            {
                if (!this.processingGeography)
                {
                    ValidateGeometryPosition(position.X, position.Y, position.Z, position.M);
                }
                this.AddControlPoint(position.X, position.Y);
                if (this.processingGeography)
                {
                    throw new FormatException(Strings.Validator_UnexpectedGeography);
                }
                return position;
            }

            protected override void OnReset()
            {
                this.InitializeObject();
            }

            protected override System.Spatial.CoordinateSystem OnSetCoordinateSystem(System.Spatial.CoordinateSystem coordinateSystem)
            {
                ValidatorState state = this.stack.Peek();
                this.Execute(PipelineCall.SetCoordinateSystem);
                if (state == CoordinateSystem)
                {
                    this.validationCoordinateSystem = coordinateSystem;
                    return coordinateSystem;
                }
                if (this.validationCoordinateSystem != coordinateSystem)
                {
                    throw new FormatException(Strings.Validator_SridMismatch);
                }
                return coordinateSystem;
            }

            private void Return()
            {
                this.stack.Pop();
            }

            private void TrackPosition(double first, double second)
            {
                if (this.pointCount == 0)
                {
                    this.initialFirstCoordinate = first;
                    this.initialSecondCoordinate = second;
                }
                this.mostRecentFirstCoordinate = first;
                this.mostRecentSecondCoordinate = second;
                this.pointCount++;
            }

            private static void ValidateGeographyPolygon(int numOfPoints, double initialFirstCoordinate, double initialSecondCoordinate, double mostRecentFirstCoordinate, double mostRecentSecondCoordinate)
            {
                if (((numOfPoints < 4) || (initialFirstCoordinate != mostRecentFirstCoordinate)) || !AreLongitudesEqual(initialSecondCoordinate, mostRecentSecondCoordinate))
                {
                    throw new FormatException(Strings.Validator_InvalidPolygonPoints);
                }
            }

            private static void ValidateGeographyPosition(double latitude, double longitude, double? z, double? m)
            {
                ValidateOnePosition(latitude, longitude, z, m);
                if (!IsLatitudeValid(latitude))
                {
                    throw new FormatException(Strings.Validator_InvalidLatitudeCoordinate(latitude));
                }
                if (!IsLongitudeValid(longitude))
                {
                    throw new FormatException(Strings.Validator_InvalidLongitudeCoordinate(longitude));
                }
            }

            private static void ValidateGeometryPolygon(int numOfPoints, double initialFirstCoordinate, double initialSecondCoordinate, double mostRecentFirstCoordinate, double mostRecentSecondCoordinate)
            {
                if (((numOfPoints < 4) || (initialFirstCoordinate != mostRecentFirstCoordinate)) || (initialSecondCoordinate != mostRecentSecondCoordinate))
                {
                    throw new FormatException(Strings.Validator_InvalidPolygonPoints);
                }
            }

            private static void ValidateGeometryPosition(double x, double y, double? z, double? m)
            {
                ValidateOnePosition(x, y, z, m);
            }

            private static void ValidateOnePosition(double first, double second, double? z, double? m)
            {
                if (!IsPointValid(first, second, z, m))
                {
                    throw new FormatException(Strings.Validator_InvalidPointCoordinate(first, second, z, m));
                }
            }

            private class BeginGeoState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    switch (transition)
                    {
                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginPoint:
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.PointStart);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginLineString:
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.LineStringStart);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginPolygon:
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.PolygonStart);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginMultiPoint:
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.MultiPoint);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginMultiLineString:
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.MultiLineString);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginMultiPolygon:
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.MultiPolygon);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginCollection:
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.Collection);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFullGlobe:
                            if (validator.depth != 1)
                            {
                                throw new FormatException(Strings.Validator_FullGlobeInCollection);
                            }
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.FullGlobe);
                            return;
                    }
                    SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.Begin, transition);
                }
            }

            private class CollectionState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    switch (transition)
                    {
                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem:
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginPoint:
                            validator.Call(SpatialValidatorImplementation.NestedValidator.PointStart);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginLineString:
                            validator.Call(SpatialValidatorImplementation.NestedValidator.LineStringStart);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginPolygon:
                            validator.Call(SpatialValidatorImplementation.NestedValidator.PolygonStart);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginMultiPoint:
                            validator.Call(SpatialValidatorImplementation.NestedValidator.MultiPoint);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginMultiLineString:
                            validator.Call(SpatialValidatorImplementation.NestedValidator.MultiLineString);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginMultiPolygon:
                            validator.Call(SpatialValidatorImplementation.NestedValidator.MultiPolygon);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginCollection:
                            validator.Call(SpatialValidatorImplementation.NestedValidator.Collection);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFullGlobe:
                            throw new FormatException(Strings.Validator_FullGlobeInCollection);

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.End:
                            validator.Return();
                            return;
                    }
                    SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem, SpatialValidatorImplementation.NestedValidator.PipelineCall.Begin, SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                }
            }

            private class FullGlobeState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    if (transition != SpatialValidatorImplementation.NestedValidator.PipelineCall.End)
                    {
                        throw new FormatException(Strings.Validator_FullGlobeCannotHaveElements);
                    }
                    validator.Return();
                }
            }

            private class LineStringBuildingState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    switch (transition)
                    {
                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.LineTo:
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.EndFigure:
                            if (validator.pointCount < 2)
                            {
                                throw new FormatException(Strings.Validator_LineStringNeedsTwoPoints);
                            }
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.LineStringEnd);
                            return;
                    }
                    SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.LineTo, SpatialValidatorImplementation.NestedValidator.PipelineCall.EndFigure, transition);
                }
            }

            private class LineStringEndState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    if (transition == SpatialValidatorImplementation.NestedValidator.PipelineCall.End)
                    {
                        validator.Return();
                    }
                    else
                    {
                        SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                    }
                }
            }

            private class LineStringStartState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    SpatialValidatorImplementation.NestedValidator.PipelineCall call = transition;
                    if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFigure)
                    {
                        validator.Jump(SpatialValidatorImplementation.NestedValidator.LineStringBuilding);
                    }
                    else if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.End)
                    {
                        validator.Return();
                    }
                    else
                    {
                        SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFigure, SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                    }
                }
            }

            private class MultiLineStringState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    SpatialValidatorImplementation.NestedValidator.PipelineCall call = transition;
                    if (call != SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem)
                    {
                        if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginLineString)
                        {
                            validator.Call(SpatialValidatorImplementation.NestedValidator.LineStringStart);
                        }
                        else if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.End)
                        {
                            validator.Return();
                        }
                        else
                        {
                            SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem, SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginLineString, SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                        }
                    }
                }
            }

            private class MultiPointState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    switch (transition)
                    {
                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem:
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginPoint:
                            validator.Call(SpatialValidatorImplementation.NestedValidator.PointStart);
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.End:
                            validator.Return();
                            return;
                    }
                    SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem, SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginPoint, SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                }
            }

            private class MultiPolygonState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    SpatialValidatorImplementation.NestedValidator.PipelineCall call = transition;
                    if (call != SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem)
                    {
                        if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginPolygon)
                        {
                            validator.Call(SpatialValidatorImplementation.NestedValidator.PolygonStart);
                        }
                        else if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.End)
                        {
                            validator.Return();
                        }
                        else
                        {
                            SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem, SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginPolygon, SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                        }
                    }
                }
            }

            private enum PipelineCall
            {
                SetCoordinateSystem,
                Begin,
                BeginPoint,
                BeginLineString,
                BeginPolygon,
                BeginMultiPoint,
                BeginMultiLineString,
                BeginMultiPolygon,
                BeginCollection,
                BeginFullGlobe,
                BeginFigure,
                LineTo,
                EndFigure,
                End
            }

            private class PointBuildingState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    switch (transition)
                    {
                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.LineTo:
                            if (validator.pointCount != 0)
                            {
                                SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.EndFigure, transition);
                            }
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.EndFigure:
                            if (validator.pointCount == 0)
                            {
                                SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFigure, transition);
                            }
                            validator.Jump(SpatialValidatorImplementation.NestedValidator.PointEnd);
                            return;
                    }
                    SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.EndFigure, transition);
                }
            }

            private class PointEndState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    if (transition == SpatialValidatorImplementation.NestedValidator.PipelineCall.End)
                    {
                        validator.Return();
                    }
                    else
                    {
                        SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                    }
                }
            }

            private class PointStartState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    SpatialValidatorImplementation.NestedValidator.PipelineCall call = transition;
                    if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFigure)
                    {
                        validator.Jump(SpatialValidatorImplementation.NestedValidator.PointBuilding);
                    }
                    else if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.End)
                    {
                        validator.Return();
                    }
                    else
                    {
                        SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFigure, SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                    }
                }
            }

            private class PolygonBuildingState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    switch (transition)
                    {
                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.LineTo:
                            return;

                        case SpatialValidatorImplementation.NestedValidator.PipelineCall.EndFigure:
                            validator.ringCount++;
                            if (!validator.processingGeography)
                            {
                                SpatialValidatorImplementation.NestedValidator.ValidateGeometryPolygon(validator.pointCount, validator.initialFirstCoordinate, validator.initialSecondCoordinate, validator.mostRecentFirstCoordinate, validator.mostRecentSecondCoordinate);
                                break;
                            }
                            SpatialValidatorImplementation.NestedValidator.ValidateGeographyPolygon(validator.pointCount, validator.initialFirstCoordinate, validator.initialSecondCoordinate, validator.mostRecentFirstCoordinate, validator.mostRecentSecondCoordinate);
                            break;

                        default:
                            SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.LineTo, SpatialValidatorImplementation.NestedValidator.PipelineCall.EndFigure, transition);
                            return;
                    }
                    validator.Jump(SpatialValidatorImplementation.NestedValidator.PolygonStart);
                }
            }

            private class PolygonStartState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    SpatialValidatorImplementation.NestedValidator.PipelineCall call = transition;
                    if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFigure)
                    {
                        validator.Jump(SpatialValidatorImplementation.NestedValidator.PolygonBuilding);
                    }
                    else if (call == SpatialValidatorImplementation.NestedValidator.PipelineCall.End)
                    {
                        validator.ringCount = 0;
                        validator.Return();
                    }
                    else
                    {
                        SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.BeginFigure, SpatialValidatorImplementation.NestedValidator.PipelineCall.End, transition);
                    }
                }
            }

            private class SetCoordinateSystemState : SpatialValidatorImplementation.NestedValidator.ValidatorState
            {
                internal override void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator)
                {
                    if (transition == SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem)
                    {
                        validator.Call(SpatialValidatorImplementation.NestedValidator.BeginSpatial);
                    }
                    else
                    {
                        SpatialValidatorImplementation.NestedValidator.ValidatorState.ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall.SetCoordinateSystem, transition);
                    }
                }
            }

            private abstract class ValidatorState
            {
                protected ValidatorState()
                {
                }

                protected static void ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator.PipelineCall actual)
                {
                    throw new FormatException(Strings.Validator_UnexpectedCall(transition, actual));
                }

                protected static void ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall transition1, SpatialValidatorImplementation.NestedValidator.PipelineCall transition2, SpatialValidatorImplementation.NestedValidator.PipelineCall actual)
                {
                    throw new FormatException(Strings.Validator_UnexpectedCall2(transition1, transition2, actual));
                }

                protected static void ThrowExpected(SpatialValidatorImplementation.NestedValidator.PipelineCall transition1, SpatialValidatorImplementation.NestedValidator.PipelineCall transition2, SpatialValidatorImplementation.NestedValidator.PipelineCall transition3, SpatialValidatorImplementation.NestedValidator.PipelineCall actual)
                {
                    throw new FormatException(Strings.Validator_UnexpectedCall2(transition1 + ", " + transition2, transition3, actual));
                }

                internal abstract void ValidateTransition(SpatialValidatorImplementation.NestedValidator.PipelineCall transition, SpatialValidatorImplementation.NestedValidator validator);
            }
        }
    }
}


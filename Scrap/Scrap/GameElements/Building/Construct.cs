﻿using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scrap.GameElements.Building;
using Scrap.GameElements.GameWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrap.GameElements.Entities
{

    

    [Serializable]
    public abstract class Construct
    {
        protected List<Joint> joints;
        protected Segment keyObject;
        public Dictionary<Point, ConstructElement> buildElements;
        public Dictionary<Point, Sensor> validPositions;
        protected ScrapGame game;
        

        public Segment KeyObject
        {
            get { return keyObject; }
            set 
            {
                keyObject = value;
                buildElements.Add(Point.Zero,keyObject.constructElement); 
            }
        }
        public Construct(ScrapGame game)
        {
            this.game = game;
            joints = new List<Joint>();
            buildElements = new Dictionary<Point,ConstructElement>();
            game.constructList.Add(this);
            validPositions = new Dictionary<Point, Sensor>();

        }
        public virtual void Update(GameTime gameTime)
        {
            foreach (var item in buildElements.Values)
            {
                item.Update();
            }

        }
        public void RecalculateAdjacentSegmentsAndActivateSensors()
        {
            foreach (ConstructElement item in buildElements.Values)
            {
                item.EnableSensors();
            }
        }
        public void SetSegmentDirection(Segment segment, Direction direction)
        {
            var recievingSegment = this.buildElements[segment.constructElement.adjacentElements.First()].segment;
            var offset = segment.constructElement.offSet - recievingSegment.constructElement.offSet;
            this.FreeObject(segment);
            AddNewSegmentToConstruct(recievingSegment, segment, offset, direction);
        }
        public bool ContainsFixture(Fixture a)
        {
            foreach (ConstructElement entity in buildElements.Values)
            {
                if (entity.segment.body.FixtureList.Contains(a))
                {
                    return true;
                }
            }
            return false;
        }
        public bool ContainsSegment(Segment segment)
        {
            foreach (ConstructElement element in this.buildElements.Values)
            {
                if (element.segment == segment)
                    return true;
            }
            return false;
        }
        public List<Point> AdjacentElements(Point position)
        {
            List<Point> adjacentElements = new List<Point>();
            Point gridOffset = position + Orientation.DirectionToPoint(Direction.Up);

            if (buildElements.Keys.Contains<Point>(gridOffset))
            {
                adjacentElements.Add(gridOffset);
            }
            gridOffset = position + Orientation.DirectionToPoint(Direction.Down);
            if (buildElements.Keys.Contains<Point>(gridOffset))
            {
                adjacentElements.Add(gridOffset);
            }
            gridOffset = position + Orientation.DirectionToPoint(Direction.Left);
            if (buildElements.Keys.Contains<Point>(gridOffset))
            {
                adjacentElements.Add(gridOffset);
            }
            gridOffset = position + Orientation.DirectionToPoint(Direction.Right);
            if (buildElements.Keys.Contains<Point>(gridOffset))
            {
                adjacentElements.Add(gridOffset);
            }

            return adjacentElements;
        }
        public Dictionary<Point, Sensor> GetValidJoinPositions()
        {
            Dictionary<Point, Sensor> validLocations = new Dictionary<Point, Sensor>();
            foreach(ConstructElement element in this.buildElements.Values)
            {
                element.GetValidJoinPositions().ToList().ForEach(x => validLocations[x.Key] = x.Value);
            }
            return validLocations;
        }
        public void AddSegmentAtSensorPosition(Segment segment, Sensor sensor)
        {//Segment will point up until the user picks the orientation 

            AddNewSegmentToConstruct(sensor.constructElement.segment, segment, sensor.GetOffsetRelativeToConstruct(), Direction.Up);
            RecalculateAdjacentSegmentsAndActivateSensors();
        }
        public void AttachSegmenAtSensorAndOrientateCorrectly(ConstructElement constructElement, Sensor sensor)
        {
            constructElement.segment.body.Rotation = sensor.body.Rotation;
            constructElement.segment.body.Position = sensor.constructElement.segment.Position - (sensor.constructElement.segment.Position - sensor.body.Position) * 2;
            constructElement.segment.body.LinearVelocity = sensor.body.LinearVelocity;


            this.game.hudButtonMapping.AddSegment(constructElement.segment);
            sensor.constructElement.construct.AddSegmentAtSensorPosition(constructElement.segment, sensor);

            constructElement.Status = ElementStatus.Locked;


        }
        public virtual void Draw(SpriteBatch batch)
        {
            foreach (ConstructElement item in this.buildElements.Values)
            {
                item.Draw(batch);
            }
        }
        protected void AddNewSegmentToConstruct(Segment recievingSegment, Segment newSegment, Point relativeOffset, Direction direction)
        {
            float rotation = Orientation.DirectionToRadians(direction);

            Vector2 anchorOffset;
            Joint joint;
            
            newSegment.body.SetTransform(newSegment.Position, recievingSegment.Rotation + rotation);
            Point offsetRelativeToOrientatedSegment = relativeOffset;
            anchorOffset = new Vector2((offsetRelativeToOrientatedSegment).X * 1.2f, (offsetRelativeToOrientatedSegment).Y * 1.2f);

            Debug.WriteLine("AddNewSegmentToConstruct() relative offset:" + relativeOffset.ToString());
            Debug.WriteLine("AddNewSegmentToConstruct() relative anchorOffset:" + anchorOffset.ToString());
            Debug.WriteLine("AddNewSegmentToConstruct() relative offsetRelativeToOrientatedSegment:" + offsetRelativeToOrientatedSegment.ToString());
            Point newSegmentIndex = relativeOffset + recievingSegment.constructElement.offSet;
            Debug.WriteLine("AddNewSegmentToConstruct() new index:" + newSegmentIndex.ToString());
            if (!buildElements.ContainsKey(newSegmentIndex))
            {
                Direction relativeDirectionofJoint = Orientation.PointToDirection(relativeOffset);
                relativeDirectionofJoint = CombineDirections(recievingSegment, relativeDirectionofJoint);
                anchorOffset = Orientation.GetRelativePositionOfADirection(relativeDirectionofJoint);
                joint = CreateJointBetweenAnchorsOnSegments(recievingSegment, newSegment, relativeDirectionofJoint, anchorOffset);
                newSegment.constructElement.AddToConstruct(this, newSegmentIndex, joint, recievingSegment.constructElement, direction);
                
                buildElements.Add(newSegmentIndex, newSegment.constructElement);
                newSegment.constructElement.EnableSensors();
                recievingSegment.constructElement.construct = this;
            }
            else
            {
                Debug.WriteLine("AddNewSegmentToConstruct key exists: " + (relativeOffset + recievingSegment.constructElement.offSet).ToString());
            }
        }

        private static Direction CombineDirections(Segment recievingSegment, Direction relativeDirectionofJoint)
        {
            switch (recievingSegment.constructElement.orientation)
            {
                case Direction.Up:
                    break;
                case Direction.Right:
                    switch (relativeDirectionofJoint)
                    {
                        case Direction.Up:
                            relativeDirectionofJoint = Direction.Left;
                            break;
                        case Direction.Right:
                            relativeDirectionofJoint = Direction.Up;
                            break;
                        case Direction.Down:
                            relativeDirectionofJoint = Direction.Right;
                            break;
                        case Direction.Left:
                            relativeDirectionofJoint = Direction.Down;
                            break;
                        default:
                            break;
                    }
                    break;
                case Direction.Down:
                    switch (relativeDirectionofJoint)
                    {
                        case Direction.Up:
                            relativeDirectionofJoint = Direction.Down;
                            break;
                        case Direction.Right:
                            relativeDirectionofJoint = Direction.Left;
                            break;
                        case Direction.Down:
                            relativeDirectionofJoint = Direction.Up;
                            break;
                        case Direction.Left:
                            relativeDirectionofJoint = Direction.Right;
                            break;
                        default:
                            break;
                    }
                    break;
                case Direction.Left:
                    switch (relativeDirectionofJoint)
                    {
                        case Direction.Up:
                            relativeDirectionofJoint = Direction.Right;
                            break;
                        case Direction.Right:
                            relativeDirectionofJoint = Direction.Down;
                            break;
                        case Direction.Down:
                            relativeDirectionofJoint = Direction.Left;
                            break;
                        case Direction.Left:
                            relativeDirectionofJoint = Direction.Up;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return relativeDirectionofJoint;
        }
        private Joint CreateJointBetweenAnchorsOnSegments(Segment entityA, Segment entityB, Direction direction, Vector2 anchorOffset)
        {
            Joint joint;
            //The offset is applied to the first entity in order to allow the second entity to have alternative rotations
            joint = JointFactory.CreateWeldJoint(game.world, entityA.GetJointAnchor(direction), entityB.GetJointAnchor(direction), anchorOffset, new Vector2(0, 0));
            joints.Add(joint);
            return joint;
        }


        public bool TestPlacement(Vector2 point)
        {
            return false;
        }
        public Vector2 Position
        {
            get { return KeyObject.Position; }
        }
        public void SetPosition(Vector2 pos, bool useWorldCoordinates = true)
        {
            if (useWorldCoordinates)
                pos -= KeyObject.Position;
            foreach (ConstructElement current in buildElements.Values)
            {
                current.segment.Position += pos;
            }
        }

        public void Rotate(float rot)
        {
            foreach (ConstructElement current in buildElements.Values)
            {
                current.segment.Rotation += rot;
                if (current.segment != KeyObject)
                {
                    current.segment.Position -= KeyObject.Position;
                    float cos = (float)Math.Cos(rot);
                    float sin = (float)Math.Sin(rot);
                    Vector2 rotationVector = current.segment.Position;
                    rotationVector = new Vector2(current.segment.Position.X * cos - current.segment.Position.Y * sin, current.segment.Position.X * sin + current.segment.Position.Y * cos);
                    current.segment.Position = rotationVector + KeyObject.Position;
                }
            }
        }

        public void Rotate(float rot, Vector2 pos, bool useWorldCoordinates = true)
        {//TODO: test this function (I think it has bugs)
            foreach (ConstructElement current in buildElements.Values)
            {
                current.segment.Rotation += rot;
                if(!useWorldCoordinates)
                {
                    pos += KeyObject.Position;
                }
                current.segment.Position -= pos;
                float cos = (float)Math.Cos(rot);
                float sin = (float)Math.Sin(rot);
                Vector2 rotationVector = current.segment.Position;
                rotationVector = new Vector2(current.segment.Position.X * cos - current.segment.Position.Y * sin, current.segment.Position.X * sin + current.segment.Position.Y * cos);
                current.segment.Position = rotationVector + pos;
            }
        }
        public void FreeObject(Segment entity)
        {
            entity.constructElement.RemoveFromConstruct();
        }

        public void SetRotation(float rot)
        {
            Rotate(rot - KeyObject.Rotation);
        }

        public void SetRotation(float rot, Vector2 pos, bool useWorldCoordinates = true)
        {
            Rotate(rot - KeyObject.Rotation, pos, useWorldCoordinates);
        }
    }
}

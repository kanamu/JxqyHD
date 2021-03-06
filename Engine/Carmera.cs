﻿using System;
using Engine.Map;
using Microsoft.Xna.Framework;

namespace Engine
{
    public class Carmera
    {
        private int _worldWidth;
        private int _worldHeight;
        private int _viewBeginX;
        private int _viewBeginY;
        private int _viewWidth;
        private int _viewHeight;

        private int _moveSpeed;
        private int _leftMoveFrames;
        private Vector2 _moveDirection = Vector2.Zero;
        private Vector2 _totalMovedDistance = Vector2.Zero;
        private Vector2 _lastPlayerPosition;
        private Character _lastPlayer;
        private Vector2 _moveToBeginDestination;
        //Vibrating screen begin
        private int _vibratingDegree;
        private int _xVibratingSum;
        private int _yVibratingSum;
        //Vibrating screen end

        #region Properties
        public bool IsInMove { get { return _leftMoveFrames > 0; } }
        public bool IsInMoveTo { get; private set; }
        public int ViewHeight
        {
            get { return _viewHeight > WorldHeight ? WorldHeight : _viewHeight; }
            set
            {
                if (WorldHeight > 0) // WorldHeight is setted
                    _viewHeight = (int)MathHelper.Clamp(value, 0, WorldHeight);
                else
                    _viewHeight = value < 0 ? 0 : value;
            }
        }

        public int ViewWidth
        {
            get { return _viewWidth > WorldWidth ? WorldWidth : _viewWidth; }
            set
            {
                if (WorldWidth > 0) //WorldWidth is setted
                    _viewWidth = (int)MathHelper.Clamp(value, 0, WorldWidth);
                else
                    _viewWidth = value < 0 ? 0 : value;
            }
        }

        public Vector2 ViewSize
        {
            get { return new Vector2(ViewWidth, ViewHeight); }
            set
            {
                ViewWidth = (int)value.X;
                ViewHeight = (int)value.Y;
            }
        }

        public int ViewBeginY
        {
            get { return _viewBeginY; }
            set
            {
                if (value <= 0) _viewBeginY = 0;
                else if (value + ViewHeight > WorldHeight)
                    _viewBeginY = WorldHeight - ViewHeight;
                else _viewBeginY = value;
                if (_viewBeginY < 0) _viewBeginY = 0;
            }
        }

        public int ViewBeginX
        {
            get { return _viewBeginX; }
            set
            {
                if (value <= 0) _viewBeginX = 0;
                else if (value + ViewWidth > WorldWidth)
                    _viewBeginX = WorldWidth - ViewWidth;
                else _viewBeginX = value;
                if (_viewBeginX < 0) _viewBeginX = 0;
            }
        }

        public int WorldHeight
        {
            get { return _worldHeight; }
            set
            {
                _worldHeight = value < 0 ? 0 : value;
            }
        }

        public int WorldWidth
        {
            get { return _worldWidth; }
            set
            {
                _worldWidth = value < 0 ? 0 : value;
            }
        }

        public Vector2 CarmeraBeginPositionInWorld
        {
            get { return new Vector2(ViewBeginX, ViewBeginY); }
            set
            {
                ViewBeginX = (int)value.X;
                ViewBeginY = (int)value.Y;
            }
        }

        public Rectangle CarmerRegionInWorld
        {
            get { return new Rectangle(ViewBeginX, ViewBeginY, ViewWidth, ViewHeight); }
            set
            {
                ViewBeginX = value.X;
                ViewBeginY = value.Y;
                ViewWidth = value.Width;
                ViewHeight = value.Height;
            }
        }
        #endregion Properties

        #region Ctor
        public Carmera() { }

        public Carmera(int beginX, int beginY, int viewWidth, int viewHeight, int worldWidth, int worldHeight)
        {
            Init(beginX, beginY, viewWidth, viewHeight, worldWidth, worldHeight);
        }

        public Carmera(Rectangle region, int worldWidth, int worldHeight)
        {
            Init(region.X, region.Y, region.Width, region.Height, worldWidth, worldHeight);
        }
        #endregion Ctor

        private void Init(int beginX, int beginY, int viewWidth, int viewHeight, int worldWidth, int worldHeight)
        {
            WorldWidth = worldWidth;
            WorldHeight = worldHeight;
            ViewBeginX = beginX;
            ViewBeginY = beginY;
            ViewWidth = viewWidth;
            ViewHeight = viewHeight;
        }

        private Vector2 GetHalfViewSize()
        {
            return ViewSize/2f;
        }

        private void UpdateMove()
        {
            if (_leftMoveFrames > 0)
            {
                _totalMovedDistance += _moveDirection*_moveSpeed*2;
                var distance = Vector2.Zero;
                if (_totalMovedDistance.X >= 1 || _totalMovedDistance.X <= -1)
                {
                    distance.X = (int)_totalMovedDistance.X;
                    _totalMovedDistance.X -= distance.X;
                }
                if (_totalMovedDistance.Y >= 1 || _totalMovedDistance.Y <= -1)
                {
                    distance.Y = (int)_totalMovedDistance.Y;
                    _totalMovedDistance.Y -= distance.Y;
                }
                CarmeraBeginPositionInWorld += distance;
                _leftMoveFrames -= 1;
            }
        }

        private void UpdateMoveTo()
        {
            if (IsInMoveTo)
            {
                var dir = _moveToBeginDestination - CarmeraBeginPositionInWorld;
                if (dir.Length() < 5)
                {
                    CarmeraBeginPositionInWorld = _moveToBeginDestination;
                    IsInMoveTo = false;
                    return;
                }
                dir.Normalize();
                var last = CarmeraBeginPositionInWorld;
                CarmeraBeginPositionInWorld += dir*_moveSpeed;
                if (CarmeraBeginPositionInWorld == last)
                {
                    IsInMoveTo = false;
                }
            }
        }

        private void UpdatePlayerView()
        {
            var player = Globals.PlayerKindCharacter;
            var position = player.PositionInWorld;
            if(player.Kind != (int)Character.CharacterKind.Player &&
                Globals.ThePlayer.ControledCharacter == null // Update view when in controling other
                )
            {
                //Don't update when player kind character is missing.
                _lastPlayerPosition = position;
                return;
            }

            if (_lastPlayer != player)
            {
                //Player changed, reassign last player position
                _lastPlayerPosition = position;
            }
            
            if(position == Vector2.Zero) return;
            var halfView = GetHalfViewSize();
            var center = halfView +
                         new Vector2(ViewBeginX, ViewBeginY);
            var offset = position - _lastPlayerPosition;
            if (offset != Vector2.Zero)
            {
                if ((offset.X > 0 && position.X > center.X) ||
                    (offset.X < 0 && position.X < center.X))
                {
                    center.X = position.X;
                }
                if ((offset.Y > 0 && position.Y > center.Y) ||
                         (offset.Y < 0 && position.Y < center.Y))
                {
                    center.Y = position.Y;
                }
            }
            CarmeraBeginPositionInWorld = center - halfView;

            _lastPlayerPosition = position;
            _lastPlayer = player;
        }

        void UpdateVibratingScreen()
        {
            if (_vibratingDegree <= 0) return;

            var xSign = (Globals.TheRandom.Next(2) == 0 ? -1 : 1);
            var ySign = (Globals.TheRandom.Next(2) == 0 ? -1 : 1);
            var xAdd = xSign*Globals.TheRandom.Next(_vibratingDegree + 1);
            var yAdd = ySign*Globals.TheRandom.Next(_vibratingDegree + 1);
            if (Math.Abs(_xVibratingSum) > _vibratingDegree)
            {
                xAdd = -(_xVibratingSum/Math.Abs(_xVibratingSum))*Math.Abs(xAdd);
            }
            if (Math.Abs(_yVibratingSum) > _vibratingDegree)
            {
                yAdd = -(_yVibratingSum/Math.Abs(_yVibratingSum))*Math.Abs(yAdd);
            }
            CarmeraBeginPositionInWorld += new Vector2(xAdd, yAdd);
            _xVibratingSum += xAdd;
            _yVibratingSum += yAdd;
            _vibratingDegree--;
            if (_vibratingDegree == 0)
            {
                //Vibration finsh, minus back
                CarmeraBeginPositionInWorld -= new Vector2(_xVibratingSum, _yVibratingSum);
                _xVibratingSum = 0;
                _yVibratingSum = 0;
            }
        }

        #region Public method
        public void Update(GameTime gameTime)
        {
            UpdateMove();
            UpdateMoveTo();
            UpdatePlayerView();
            UpdateVibratingScreen();
        }

        public void CenterPlayerInCamera()
        {
            var position = Globals.PlayerPositionInWorld;
            CarmeraBeginPositionInWorld = position -
                                          GetHalfViewSize();
            MapBase.Instance.ViewBeginX = ViewBeginX;
            MapBase.Instance.ViewBeginY = ViewBeginY;
        }

        /// <summary>
        /// Move carmera
        /// </summary>
        /// <param name="keepFrameCount">Keep frame count.</param>
        /// <param name="speed">Pixel speed per frame</param>
        /// <param name="direction">Move direction</param>
        public void MoveTo(Vector2 direction, int keepFrameCount, int speed)
        {
            if(direction == Vector2.Zero) return;
            direction.Normalize();
            _leftMoveFrames = keepFrameCount;
            _moveSpeed = speed;
            _moveDirection = direction;
            _totalMovedDistance = Vector2.Zero;
        }

        /// <summary>
        /// Move to ceter tile position in speed
        /// </summary>
        /// <param name="centerTilePosition"></param>
        /// <param name="speed"></param>
        public void MoveTo(Vector2 centerTilePosition, int speed)
        {
            _moveToBeginDestination = MapBase.ToPixelPosition(centerTilePosition) -
                GetHalfViewSize();
            _moveSpeed = speed;
            IsInMoveTo = true;
        }

        /// <summary>
        /// Vibrating screen.
        /// </summary>
        /// <param name="degree"></param>
        public void VibaratingScreen(int degree)
        {
            _vibratingDegree = degree;
        }

        public Vector2 ToViewPosition(Vector2 worldPosition)
        {
            return worldPosition - CarmeraBeginPositionInWorld;
        }

        public Vector2 ToWorldPosition(Vector2 viewPosition)
        {
            return CarmeraBeginPositionInWorld + viewPosition;
        }

        public Rectangle ToViewRegion(Rectangle worldRegion)
        {
            Vector2 pos = ToViewPosition(new Vector2(worldRegion.X, worldRegion.Y));
            return new Rectangle((int)pos.X, (int)pos.Y, worldRegion.Width, worldRegion.Height);
        }

        public Rectangle ToWorldRegion(Rectangle viewRegion)
        {
            Vector2 pos = ToWorldPosition(new Vector2(viewRegion.X, viewRegion.Y));
            return new Rectangle((int)pos.X, (int)pos.Y, viewRegion.Width, viewRegion.Height);
        }
        #endregion Public method
    }
}

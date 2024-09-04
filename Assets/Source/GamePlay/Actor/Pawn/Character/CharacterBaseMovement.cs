using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using CMF;

/// <summary>
/// 角色基类分部脚本
/// 用于定义角色的移动行为
/// </summary>
//[RequireComponent(typeof(Mover))]
public partial class CharacterBase : ACharacter
{
	public delegate void VectorEvent(Vector3 v);

	/// <summary>
	/// 当跳跃时
	/// </summary>
	private VectorEvent m_OnJumpCharacterBase;

	/// <summary>
	/// 当着陆时
	/// </summary>
	private VectorEvent m_OnLandCharacterBase;

	//References to attached components;
	protected Mover m_Mover;
	protected CeilingDetector ceilingDetector;

	//Jump key variables;
	private bool jumpInputIsLocked = false;
	private bool jumpKeyWasPressed = false;
	private bool jumpKeyWasLetGo = false;
	private bool jumpKeyIsPressed = false;

	[Header("Movement : ")]
	//Movement speed;
	[SerializeField] private float movementSpeed = 1.8f;

	//How fast the controller can change direction while in the air;
	//Higher values result in more air control;
	[SerializeField] private float airControlRate = 0.5f;

	//Jump speed;
	[SerializeField] private float jumpSpeed = 2.8f;

	//Jump duration variables;
	[SerializeField] private float jumpDuration = 0.2f;
	float currentJumpStartTime = 0f;

	//'AirFriction' determines how fast the controller loses its momentum while in the air;
	//'GroundFriction' is used instead, if the controller is grounded;
	[SerializeField] private float airFriction = 0.5f;
	[SerializeField] private float groundFriction = 100f;

	//Current momentum;
	protected Vector3 momentum = Vector3.zero;

	//Saved velocity from last frame;
	Vector3 savedVelocity = Vector3.zero;

	//Saved horizontal movement velocity from last frame;
	Vector3 savedMovementVelocity = Vector3.zero;

	//Amount of downward gravity;
	[Header("重力")]
	[SerializeField] private bool m_UseGravity = true; //应用重力
    [SerializeField] private float gravity = 30f;
	[Tooltip("How fast the character will slide down steep slopes.")]
	[SerializeField] private float slideGravity = 1f;

    //Acceptable slope angle limit;
    [Header("可行动斜角度最大值")]
    [SerializeField] private float slopeLimit = 80f;

	[Tooltip("Whether to calculate and apply momentum relative to the controller's transform.")]
	[SerializeField] private bool useLocalMomentum = false;

	/// <summary>
	/// 玩家移动状态
	/// </summary>
	public enum MovementState
	{
		Grounded,
		Sliding,
		Falling,
		Rising,
		Jumping
	}

	MovementState currentMovementState = MovementState.Falling;

	[Tooltip("Optional camera transform used for calculating movement direction. If assigned, character movement will take camera view into account.")]
	[SerializeField] private Transform cameraTransform;

	#region MoveInfo
	/// <summary>
	/// 获取移动方向
	/// </summary>
	public Vector3 GetMoveDir
	{
		get
		{
			if (m_MoveDirFixed != Vector3.zero)
                return m_MoveDirFixed.normalized;
            else
                return m_InputInfoComponent.MoveDir.Direction;
        }
    }

	/// <summary>
	/// 获取移动方向水平输入量
	/// </summary>
	public float GetMoveDirHorizontal
	{
		get
		{
			if (m_MoveDirFixed != Vector3.zero)
                return m_MoveDirFixed.x;
            else
                return m_InputInfoComponent.MoveDir.Horizontal;
        }
    }

	/// <summary>
	/// 获取移动方向垂直输入量
	/// </summary>
	public float GetMoveDirVertical
	{
		get
		{
			if (m_MoveDirFixed != Vector3.zero)
                return m_MoveDirFixed.z;
            else
                return m_InputInfoComponent.MoveDir.Vertical;
        }
    }

	/// <summary>
	/// 有移动方向
	/// </summary>
	public bool IsHaveMoveDir
    {
        get { return GetMoveDir != Vector3.zero; }
	}

	private Vector3 m_MoveDirFixed;

	/// <summary>
	/// 设置移动方向，固定
	/// 此值将优先作为移动方向数据进行获取，为零时获取输入方向
	/// </summary>
	/// <param name="moveDirFixed"></param>
	private void SetMoveDirFixed(Vector3 moveDirFixed)
    {
		m_MoveDirFixed = moveDirFixed;
	}

	private void ClearMoveDirFixed()
    {
		m_MoveDirFixed = Vector3.zero;
	}
	#endregion

	public void InitCharacterBaseMovement()
    {
		m_Mover = GetComponent<Mover>();
		ceilingDetector = GetComponent<CeilingDetector>();
	}

	public void TickCharacterBaseMovement(float deltaTime)
    {
		HandleJumpKeyInput();
	}

	void FixedTickCharacterBaseMovement(float fixedDeltaTime)
	{
		ControllerMovementUpdate();
	}

	//跳跃是否输入
	void HandleJumpKeyInput()
	{
		bool _newJumpKeyPressedState = IsJumpKeyClick();

		if (jumpKeyIsPressed == false && _newJumpKeyPressedState == true)
			jumpKeyWasPressed = true;

		if (jumpKeyIsPressed == true && _newJumpKeyPressedState == false)
		{
			jumpKeyWasLetGo = true;
			jumpInputIsLocked = false;
		}

		jumpKeyIsPressed = _newJumpKeyPressedState;
	}

	//控制移动更新，必须在FixedUpdate中执行
	void ControllerMovementUpdate()
	{
		m_Mover.CheckForGround();//确认在地面

		currentMovementState = DetermineMovementState();//确认状态

		HandleMomentum();//应用摩擦和重力到推力

		HandleJumping();//确认玩家是否跳跃

		//Calculate movement velocity;
		Vector3 _velocity = Vector3.zero;
		if (currentMovementState == MovementState.Grounded)
			_velocity = CalculateMovementVelocity();

		//If local momentum is used, transform momentum into world space first;
		Vector3 _worldMomentum = momentum;
		if (useLocalMomentum)
			_worldMomentum = TransformGet.localToWorldMatrix * momentum;

		//Add current momentum to velocity;
		_velocity += _worldMomentum;

		//If player is grounded or sliding on a slope, extend mover's sensor range;
		//This enables the player to walk up/down stairs and slopes without losing ground contact;
		m_Mover.SetExtendSensorRange(IsGrounded());

		//Set mover velocity;		
		m_Mover.SetVelocity(_velocity);

		//Store velocity for next frame;
		savedVelocity = _velocity;

		//Save controller movement velocity;
		savedMovementVelocity = CalculateMovementVelocity();

		//Reset jump key booleans;
		jumpKeyWasLetGo = false;
		jumpKeyWasPressed = false;

		//Reset ceiling detector, if one is attached to this gameobject;
		if (ceilingDetector != null)
			ceilingDetector.ResetFlags();
	}

	//Calculate and return movement direction based on player input;
	//This function can be overridden by inheriting scripts to implement different player controls;
	protected virtual Vector3 CalculateMovementDirection()
	{
		//If no character input script is attached to this object, return;
		if (m_InputInfoComponent == null)
			return Vector3.zero;

		Vector3 _velocity = Vector3.zero;

		//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
		if (cameraTransform == null)
		{
			Vector3 dir = GetMoveDir;
			_velocity = new Vector3(dir.x, dir.y, dir.z);
		}
		else
		{
			//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
			//Project movement direction so movement stays parallel to the ground;
			_velocity += Vector3.ProjectOnPlane(cameraTransform.right, TransformGet.up).normalized * GetMoveDirHorizontal;
			_velocity += Vector3.ProjectOnPlane(cameraTransform.forward, TransformGet.up).normalized * GetMoveDirVertical;
		}

		//If necessary, clamp movement vector to magnitude of 1f;
		if (_velocity.magnitude > 1f)
			_velocity.Normalize();

		return _velocity;
	}

	//Calculate and return movement velocity based on player input, controller state, ground normal [...];
	protected virtual Vector3 CalculateMovementVelocity()
	{
		//Calculate (normalized) movement direction;
		Vector3 _velocity = CalculateMovementDirection();

		//Multiply (normalized) velocity with movement speed;
		_velocity *= movementSpeed;

		return _velocity;
	}

	//Returns 'true' if the player presses the jump key;
	protected virtual bool IsJumpKeyClick()
	{
		//If no character input script is attached to this object, return;
		if (m_InputInfoComponent == null)
			return false;

		return m_InputInfoComponent.JumpBtn.IsClick;
	}

	//控制器状态
	private MovementState DetermineMovementState()
	{
		//Determine current controller state based on current momentum and whether the controller is grounded (or not)

		//Check if vertical momentum is pointing upwards;
		bool _isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), TransformGet.up) > 0f);
		//Check if controller is sliding;
		bool _isSliding = m_Mover.IsGrounded() && IsGroundTooSteep();

		//Grounded;
		if (currentMovementState == MovementState.Grounded)
		{
			if (_isRising)
			{
				OnGroundContactLost();
				return MovementState.Rising;
			}
			if (!m_Mover.IsGrounded())
			{
				OnGroundContactLost();
				return MovementState.Falling;
			}
			if (_isSliding)
			{
				OnGroundContactLost();
				return MovementState.Sliding;
			}
			return MovementState.Grounded;
		}

		//Falling;
		if (currentMovementState == MovementState.Falling)
		{
			if (_isRising)
			{
				return MovementState.Rising;
			}
			if (m_Mover.IsGrounded() && !_isSliding)
			{
				OnGroundContactRegained();
				return MovementState.Grounded;
			}
			if (_isSliding)
			{
				return MovementState.Sliding;
			}
			return MovementState.Falling;
		}

		//Sliding;
		if (currentMovementState == MovementState.Sliding)
		{
			if (_isRising)
			{
				OnGroundContactLost();
				return MovementState.Rising;
			}
			if (!m_Mover.IsGrounded())
			{
				OnGroundContactLost();
				return MovementState.Falling;
			}
			if (m_Mover.IsGrounded() && !_isSliding)
			{
				OnGroundContactRegained();
				return MovementState.Grounded;
			}
			return MovementState.Sliding;
		}

		//Rising;
		if (currentMovementState == MovementState.Rising)
		{
			if (!_isRising)
			{
				if (m_Mover.IsGrounded() && !_isSliding)
				{
					OnGroundContactRegained();
					return MovementState.Grounded;
				}
				if (_isSliding)
				{
					return MovementState.Sliding;
				}
				if (!m_Mover.IsGrounded())
				{
					return MovementState.Falling;
				}
			}

			//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
			if (ceilingDetector != null)
			{
				if (ceilingDetector.HitCeiling())
				{
					OnCeilingContact();
					return MovementState.Falling;
				}
			}
			return MovementState.Rising;
		}

		//Jumping;
		if (currentMovementState == MovementState.Jumping)
		{
			//Check for jump timeout;
			if ((Time.time - currentJumpStartTime) > jumpDuration)
				return MovementState.Rising;

			//Check if jump key was let go;
			if (jumpKeyWasLetGo)
				return MovementState.Rising;

			//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
			if (ceilingDetector != null)
			{
				if (ceilingDetector.HitCeiling())
				{
					OnCeilingContact();
					return MovementState.Falling;
				}
			}
			return MovementState.Jumping;
		}

		return MovementState.Falling;
	}

	//Check if player has initiated a jump;
	void HandleJumping()
	{
		if (currentMovementState == MovementState.Grounded)
		{
			if ((jumpKeyIsPressed == true || jumpKeyWasPressed) && !jumpInputIsLocked)
			{
				//Call events;
				OnGroundContactLost();
				OnJumpStart();

				currentMovementState = MovementState.Jumping;
			}
		}
	}

	//Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
	//Handle movement in the air;
	//Handle sliding down steep slopes;
	void HandleMomentum()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if (useLocalMomentum)
			momentum = TransformGet.localToWorldMatrix * momentum;

		Vector3 _verticalMomentum = Vector3.zero;
		Vector3 _horizontalMomentum = Vector3.zero;

		//Split momentum into vertical and horizontal components;
		if (momentum != Vector3.zero)
		{
			_verticalMomentum = VectorMath.ExtractDotVector(momentum, TransformGet.up);
			_horizontalMomentum = momentum - _verticalMomentum;
		}

		//Add gravity to vertical momentum;
		if (m_UseGravity == true)
			_verticalMomentum -= TransformGet.up * gravity * Time.deltaTime;

		//Remove any downward force if the controller is grounded;
		if (currentMovementState == MovementState.Grounded && VectorMath.GetDotProduct(_verticalMomentum, TransformGet.up) < 0f)
			_verticalMomentum = Vector3.zero;

		//Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
		if (!IsGrounded())
		{
			Vector3 _movementVelocity = CalculateMovementVelocity();

			//If controller has received additional momentum from somewhere else;
			if (_horizontalMomentum.magnitude > movementSpeed)
			{
				//Prevent unwanted accumulation of speed in the direction of the current momentum;
				if (VectorMath.GetDotProduct(_movementVelocity, _horizontalMomentum.normalized) > 0f)
					_movementVelocity = VectorMath.RemoveDotVector(_movementVelocity, _horizontalMomentum.normalized);

				//Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
				float _airControlMultiplier = 0.25f;
				_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate * _airControlMultiplier;
			}
			//If controller has not received additional momentum;
			else
			{
				//Clamp _horizontal velocity to prevent accumulation of speed;
				_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate;
				_horizontalMomentum = Vector3.ClampMagnitude(_horizontalMomentum, movementSpeed);
			}
		}

		//Steer controller on slopes;
		if (currentMovementState == MovementState.Sliding)
		{
			//Calculate vector pointing away from slope;
			Vector3 _pointDownVector = Vector3.ProjectOnPlane(m_Mover.GetGroundNormal(), TransformGet.up).normalized;

			//Calculate movement velocity;
			Vector3 _slopeMovementVelocity = CalculateMovementVelocity();
			//Remove all velocity that is pointing up the slope;
			_slopeMovementVelocity = VectorMath.RemoveDotVector(_slopeMovementVelocity, _pointDownVector);

			//Add movement velocity to momentum;
			_horizontalMomentum += _slopeMovementVelocity * Time.fixedDeltaTime;
		}

		//Apply friction to horizontal momentum based on whether the controller is grounded;
		if (currentMovementState == MovementState.Grounded)
			_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, groundFriction, Time.deltaTime, Vector3.zero);
		else
			_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, airFriction, Time.deltaTime, Vector3.zero);

		//Add horizontal and vertical momentum back together;
		momentum = _horizontalMomentum + _verticalMomentum;

		//Additional momentum calculations for sliding;
		if (currentMovementState == MovementState.Sliding)
		{
			//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
			momentum = Vector3.ProjectOnPlane(momentum, m_Mover.GetGroundNormal());

			//Remove any upwards momentum when sliding;
			if (VectorMath.GetDotProduct(momentum, TransformGet.up) > 0f)
				momentum = VectorMath.RemoveDotVector(momentum, TransformGet.up);

			//Apply additional slide gravity;
			Vector3 _slideDirection = Vector3.ProjectOnPlane(-TransformGet.up, m_Mover.GetGroundNormal()).normalized;
			momentum += _slideDirection * slideGravity * Time.deltaTime;
		}

		//If controller is jumping, override vertical velocity with jumpSpeed;
		if (currentMovementState == MovementState.Jumping)
		{
			momentum = VectorMath.RemoveDotVector(momentum, TransformGet.up);
			momentum += TransformGet.up * jumpSpeed;
		}

		if (useLocalMomentum)
			momentum = TransformGet.worldToLocalMatrix * momentum;
	}

	//Events;

	//This function is called when the player has initiated a jump;
	void OnJumpStart()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if (useLocalMomentum)
			momentum = TransformGet.localToWorldMatrix * momentum;

		//Add jump force to momentum;
		momentum += TransformGet.up * jumpSpeed;

		//Set jump start time;
		currentJumpStartTime = Time.time;

		//Lock jump input until jump key is released again;
		jumpInputIsLocked = true;

		//Call event;
		if (m_OnJumpCharacterBase != null)
			m_OnJumpCharacterBase(momentum);

		if (useLocalMomentum)
			momentum = TransformGet.worldToLocalMatrix * momentum;
	}

	//This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
	void OnGroundContactLost()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if (useLocalMomentum)
			momentum = TransformGet.localToWorldMatrix * momentum;

		//Get current movement velocity;
		Vector3 _velocity = GetMovementVelocity();

		//Check if the controller has both momentum and a current movement velocity;
		if (_velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
		{
			//Project momentum onto movement direction;
			Vector3 _projectedMomentum = Vector3.Project(momentum, _velocity.normalized);
			//Calculate dot product to determine whether momentum and movement are aligned;
			float _dot = VectorMath.GetDotProduct(_projectedMomentum.normalized, _velocity.normalized);

			//If current momentum is already pointing in the same direction as movement velocity,
			//Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
			if (_projectedMomentum.sqrMagnitude >= _velocity.sqrMagnitude && _dot > 0f)
				_velocity = Vector3.zero;
			else if (_dot > 0f)
				_velocity -= _projectedMomentum;
		}

		//Add movement velocity to momentum;
		momentum += _velocity;

		if (useLocalMomentum)
			momentum = TransformGet.worldToLocalMatrix * momentum;
	}

	//This function is called when the controller has landed on a surface after being in the air;
	void OnGroundContactRegained()
	{
		//Call 'OnLand' event;
		if (m_OnLandCharacterBase != null)
		{
			Vector3 _collisionVelocity = momentum;
			//If local momentum is used, transform momentum into world coordinates first;
			if (useLocalMomentum)
				_collisionVelocity = TransformGet.localToWorldMatrix * _collisionVelocity;

			m_OnLandCharacterBase(_collisionVelocity);
		}
	}

	//This function is called when the controller has collided with a ceiling while jumping or moving upwards;
	void OnCeilingContact()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if (useLocalMomentum)
			momentum = TransformGet.localToWorldMatrix * momentum;

		//Remove all vertical parts of momentum;
		momentum = VectorMath.RemoveDotVector(momentum, TransformGet.up);

		if (useLocalMomentum)
			momentum = TransformGet.worldToLocalMatrix * momentum;
	}

	//Helper functions;

	//Returns 'true' if vertical momentum is above a small threshold;
	private bool IsRisingOrFalling()
	{
		//Calculate current vertical momentum;
		Vector3 _verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), TransformGet.up);

		//Setup threshold to check against;
		//For most applications, a value of '0.001f' is recommended;
		float _limit = 0.001f;

		//Return true if vertical momentum is above '_limit';
		return (_verticalMomentum.magnitude > _limit);
	}

	//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
	private bool IsGroundTooSteep()
	{
		if (!m_Mover.IsGrounded())
			return true;

		return (Vector3.Angle(m_Mover.GetGroundNormal(), TransformGet.up) > slopeLimit);
	}

	//Getters;

	//Get last frame's velocity;
	public Vector3 GetVelocity()
	{
		return savedVelocity;
	}

	//Get last frame's movement velocity (momentum is ignored);
	public Vector3 GetMovementVelocity()
	{
		return savedMovementVelocity;
	}

	//Get current momentum;
	public Vector3 GetMomentum()
	{
		Vector3 _worldMomentum = momentum;
		if (useLocalMomentum)
			_worldMomentum = TransformGet.localToWorldMatrix * momentum;

		return _worldMomentum;
	}

	//Returns 'true' if controller is grounded (or sliding down a slope);
	public bool IsGrounded()
	{
		return (currentMovementState == MovementState.Grounded || currentMovementState == MovementState.Sliding);
	}

	//Returns 'true' if controller is sliding;
	public bool IsSliding()
	{
		return (currentMovementState == MovementState.Sliding);
	}

	//Add momentum to controller;
	public void AddMomentum(Vector3 _momentum)
	{
		if (useLocalMomentum)
			momentum = TransformGet.localToWorldMatrix * momentum;

		momentum += _momentum;

		if (useLocalMomentum)
			momentum = TransformGet.worldToLocalMatrix * momentum;
	}

	//Set controller momentum directly;
	public void SetMomentum(Vector3 _newMomentum)
	{
		if (useLocalMomentum)
			momentum = TransformGet.worldToLocalMatrix * _newMomentum;
		else
			momentum = _newMomentum;
	}
}

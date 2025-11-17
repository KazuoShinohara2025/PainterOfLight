using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool interact;
		public bool attack;
		public bool lighting;
		public bool skil;
		public bool ult;


        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
        }
		public void OnAttack(InputValue value)
		{
			AttackInput(value.isPressed);
        }
		public void OnLighting(InputValue value)
		{
			LightingInput(value.isPressed);
        }
		public void OnSkil(InputValue value)
		{
			SkilInput(value.isPressed);
        }
		public void OnUlt(InputValue value)
		{
			UltInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void InteractInput(bool newSelectState)
		{
            interact = newSelectState;
        }
		public void AttackInput(bool newAttackState)
		{
			attack = newAttackState;
        }
		public void LightingInput(bool newLightingState)
		{
			lighting = newLightingState;
        }
		public void SkilInput(bool newSkilState)
		{
			skil = newSkilState;
        }
		public void UltInput(bool newUltState)
		{
			ult = newUltState;
        }
        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}
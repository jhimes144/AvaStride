using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstPersonShooter;
[ComponentCategory("Utils")]
[DataContract("SmoothFollowAndRotate")]
public class SmoothFollowAndRotate : SyncScript
{
	public Entity EntityToFollow { get; set; }
	public float Speed { get; set; } = 1;

	public override void Update()
	{
		var deltaTime = (float)this.Game.UpdateTime.Elapsed.TotalSeconds;
		var currentPosition = Entity.Transform.Position;
		var currentRotation = Entity.Transform.Rotation;

		EntityToFollow.Transform.GetWorldTransformation(out var otherPosition, out var otherRotation, out var _);

		var newPosition = Vector3.Lerp(currentPosition, otherPosition, Speed * deltaTime);
		Entity.Transform.Position = newPosition;

		Quaternion.Slerp(ref currentRotation, ref otherRotation, Speed * deltaTime, out var newRotation);
		Entity.Transform.Rotation = newRotation;
	}
}

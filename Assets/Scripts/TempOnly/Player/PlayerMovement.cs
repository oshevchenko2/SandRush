using UnityEngine;

public class PlayerMovement
{
    private readonly CharacterController _cc;
    private readonly float _speed = 6f;

    public PlayerMovement(CharacterController cc)
    {
        _cc = cc;
    }

    public void Tick()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0, v).normalized;
        _cc.SimpleMove(move * _speed);
    }
}

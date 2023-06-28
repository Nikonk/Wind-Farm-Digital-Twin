public class GameResource
{
    private InGameResource _resource;
    private int _currentAmount;

    public GameResource(InGameResource resource, int initialAmount)
    {
        _resource = resource;
        _currentAmount = initialAmount;
    }

    public InGameResource Name { get => _resource; }
    public int Amount { get => _currentAmount; }

    public void ChangeAmount(int value)
    {
        _currentAmount += value;

        if (_currentAmount < 0) 
            _currentAmount = 0;
    }
}


using System;

public interface IVisual
{
    void DisplayField(Grid grid);
    void DisplayChips(Chip[] chips, LevelSO levelSO);
    void DisplayLightning(int x, int y);
    void DestroyLightning();
    void SwapChips(Chip[] chips, int highlightedChip, int chipIndex, bool doCheck, bool quickly);
    event EventHandler MoveEnded;
    event EventHandler DestroyEnded;
    void DestroyChips(Chip[] chips);
    void MoveChip(Chip[] chips, int chipIndex, int x, int y, bool invoke);
    void AppearChip(Chip[] chips, int chipIndex, int x, int y);


}

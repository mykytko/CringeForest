using System.Collections.Generic;

namespace CringeForestLibrary;

public interface IStatisticsViewer
{
    void UpdateStatistics(Dictionary<string, int> dict);
}
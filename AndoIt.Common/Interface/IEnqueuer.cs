using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AndoIt.Common.Interface
{
	public interface IEnqueuer
	{
        ReadOnlyCollection<IEnqueable> Queue { get; }
		int ConfigTimingSecondsMaxTimerLapse { get; set; }
		void EnqueuePetitionsFromRepository(List<IEnqueable> equeableFromRepositry);
		void InsertTask(object sender, IEnqueable enqueable);
		void ReplyTasksToClient(object sender, IEnqueable toPocess);
		void DeleteTasks(object sender, IEnqueable toPocess);
		void Process();
		void Dispose();
	}	
}
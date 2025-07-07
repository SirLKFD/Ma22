using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IReviewService
    {
        void AddReview(ReviewViewModel model);
        List<ReviewViewModel> GetTrainingReviews(int trainingId);
    }
}

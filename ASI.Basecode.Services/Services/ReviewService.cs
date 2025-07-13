using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repository;
        private readonly IMapper _mapper;

        public ReviewService(IReviewRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public void AddReview(ReviewViewModel model)
        {
        
            var review = new Review();
      
            try{
                Console.WriteLine("✅ Mapping Review");
                _mapper.Map(model, review);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during mapping: {ex.Message}");
                throw;
            }
            review.CreatedTime = DateTime.Now;
            review.UpdatedTime = DateTime.Now;
            review.CreatedBy = System.Environment.UserName;
            review.UpdatedBy = System.Environment.UserName;

            _repository.AddReview(review);

            Console.WriteLine("[Service] AddReview called on repository.");
        }

        public List<ReviewViewModel> GetTrainingReviews(int trainingId) 
        {
              var reviews = _repository.GetReviews().Where(t => t.TrainingId == trainingId)
                .OrderByDescending(t => t.CreatedTime)
                .Select(t => new ReviewViewModel
                {
                    ReviewId = t.ReviewId,
                    TrainingId = t.TrainingId,
                    Title = t.Title,
                    UserReview = t.UserReview,
                    ReviewScore = t.ReviewScore,
                    AccountId = t.AccountId,
                }).ToList();

            return reviews;
          
        }

        public bool HasUserReviewed(int userId, int trainingId)
        {
            return _repository.GetReviews().Any(r => r.AccountId == userId && r.TrainingId == trainingId);
        }
    }
}

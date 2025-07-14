using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{   
    public class ReviewRepository : BaseRepository, IReviewRepository
    {
        public ReviewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<Review> GetReviews()
        {
            return this.GetDbSet<Review>();
        }

        public void AddReview(Review review)
        {
            Console.WriteLine($"[ReviewRepository] Starting AddReview: ReviewId={review.ReviewId}, Title='{review.Title}', UserReview='{review.UserReview}', ReviewScore={review.ReviewScore}, TrainingId={review.TrainingId}, AccountId={review.AccountId}");
            
            try
            {
                this.GetDbSet<Review>().Add(review);
                Console.WriteLine("✅ Review added to DbSet");
                
                UnitOfWork.SaveChanges();
                Console.WriteLine("✅ SaveChanges for Review called successfully.");
                Console.WriteLine($"✅ Review saved with ID: {review.ReviewId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in AddReview: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void UpdateReview(Review review)
        {
            this.GetDbSet<Review>().Update(review);
            UnitOfWork.SaveChanges();
            Console.WriteLine("UpdateReview SaveChanges called.");
        }




        
    }
}

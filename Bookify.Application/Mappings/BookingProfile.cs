using AutoMapper;
using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Domain.Entities;

namespace Bookify.Application.MappingProfiles
{
    // تأكد من أن هذا الملف يرث من Profile الخاص بـ AutoMapper
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            // 1. التعيين من كيان Booking (قاعدة البيانات) إلى BookingDto (الإخراج API)
            CreateMap<Booking, BookingDto>()
                // .ForMember(...) يُستخدم عادةً إذا كان اسم الخاصية مختلفًا، 
                // لكننا هنا نعتمد على أن الأسماء متطابقة (CustomerName, CustomerEmail)
                // لذلك، AutoMapper سيتعامل معها تلقائيًا.

                // يمكنك إضافة تعيينات مخصصة هنا إذا كنت تريد تضمين تفاصيل الغرفة
                // (مثل RoomName و RoomTypeName) التي أضفتها إلى BookingDto

                // تعيين CustomerName و CustomerEmail سيتم تلقائيًا لأنهما موجودان في كلا الكيانين

                // مثال على تعيين خصائص غير موجودة في الكيان مباشرة (مثل RoomTypeName)
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room.Id))
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.Room.RoomType.Name));

            // ملاحظة: لكي تعمل تعيينات Room و RoomType، يجب التأكد من جلبها (Include) 
            // في الـ Repository عند استدعاء Booking (كما هو الحال في GetUserBookingsAsync).


            // 2. التعيين من BookingCreateDto (الإدخال API) إلى كيان Booking (قاعدة البيانات)
            // هذا التعيين لا يُستخدم مباشرة في BookingService، حيث يتم إنشاء الكيان يدويًا،
            // ولكنه مفيد إذا كنت تستخدم AutoMapper لإنشاء الكيان بالكامل.
            CreateMap<BookingCreateDto, Booking>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.NumberOfNights, opt => opt.Ignore()) // يتم حسابها لاحقًا في الـ Service
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore());    // يتم حسابها لاحقًا في الـ Service

            // 3. التعيين من CartItemDto إلى BookingCreateDto (لأغراض تأكيد الحجز من السلة)
            CreateMap<CartItemDto, BookingCreateDto>()
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => src.CheckInDate))
                .ForMember(dest => dest.CheckOutDate, opt => opt.MapFrom(src => src.CheckOutDate))
                // ملاحظة: CartItemDto لا يحتوي على CustomerName/CustomerEmail، لذا ستحتاج
                // إلى إضافتهما يدوياً في الـ Controller قبل استدعاء BookingService إذا كنت تعتمد على هذا التعيين!
                .ReverseMap(); // تعيين في الاتجاهين
        }
    }
}
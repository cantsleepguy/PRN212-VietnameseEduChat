# VietnameseEduChat Role Task-First Visual Optimization

## Mục tiêu

Tối ưu hình ảnh và bố cục của VietnameseEduChat cho bốn vai trò `Student`, `Lecturer`, `AcademicAdmin` và `SystemAdmin` theo hướng task-first. Thay đổi giúp người dùng nhận ra tác vụ quan trọng nhanh hơn, quét nội dung dễ hơn và thao tác tốt trên desktop lẫn mobile.

Phạm vi chỉ bao gồm cách trình bày. Không thay đổi nghiệp vụ, endpoint, database, dữ liệu, authentication, authorization hoặc quyền truy cập hiện có.

## Hướng thiết kế đã chọn

Sử dụng hướng A — task-first theo role:

- Đưa 3–5 tác vụ thường dùng nhất của từng role lên vùng đầu trang.
- Nội dung tham khảo, thống kê và dữ liệu phụ xuất hiện sau tác vụ chính.
- Dùng một app shell và design system thống nhất, nhưng thay đổi thứ tự ưu tiên nội dung theo role.
- Giữ phong cách học thuật hiện đại: nền sáng, sidebar navy và teal cho CTA chính.

## Bố cục theo role

### Student

Ưu tiên theo thứ tự:

1. Hỏi đáp với AI.
2. Tiếp tục hội thoại gần đây.
3. Xem gói đang sử dụng và giới hạn liên quan.
4. Truy cập tài liệu hoặc nội dung học gần đây nếu trang hiện có dữ liệu đó.

Không tạo số liệu hoặc nội dung giả để lấp dashboard.

### Lecturer

Ưu tiên theo thứ tự:

1. Môn học được phân công.
2. Tải tài liệu cho môn học.
3. Theo dõi trạng thái xử lý tài liệu.
4. Mở Chat trong ngữ cảnh môn học.

Các môn và chương được trình bày theo cấu trúc dễ quét, với action gắn sát đối tượng liên quan.

### AcademicAdmin

Ưu tiên theo thứ tự:

1. Quản lý môn học.
2. Quản lý ngân hàng câu hỏi nghiên cứu.
3. Theo dõi và chạy thí nghiệm.
4. Truy cập cấu hình chunking theo quyền hiện có.

Các màn hình dữ liệu dày ưu tiên khả năng quét, filter và trạng thái hơn phần trang trí.

### SystemAdmin

Ưu tiên theo thứ tự:

1. Quản lý người dùng.
2. Quản lý gói dịch vụ.
3. Theo dõi thanh toán.
4. Xem các khu vực quản trị hệ thống hiện có.

Bảng quản trị dùng mật độ vừa phải, nhóm action nhất quán và làm rõ destructive action bằng chữ, icon và màu semantic.

## App shell và điều hướng

- Giữ sidebar role-aware; chỉ hiển thị chức năng người dùng có quyền truy cập.
- Nhóm navigation theo mục tiêu thay vì liệt kê phẳng khi số lượng mục lớn.
- Active state rõ bằng nền, icon và typography; không phụ thuộc duy nhất vào màu.
- Khu vực tài khoản và đăng xuất đặt cuối sidebar.
- Mobile chuyển sidebar thành drawer có overlay, Escape để đóng và focus rõ.
- Mỗi trang có đúng một `h1`, mô tả ngắn và tối đa một CTA chính trong page header.

## Visual hierarchy

- Giảm pattern “mọi nội dung đều là card”.
- Dùng card cho nội dung độc lập hoặc tương tác; dùng section, divider và khoảng trắng cho cấu trúc thông thường.
- Giảm shadow; dùng border nhẹ và surface contrast để phân lớp.
- Duy trì spacing scale 4px với các mức 4, 8, 12, 16, 24, 32 và 48px.
- Dùng radius nhỏ cho input/badge, vừa cho card và lớn chỉ cho khu vực nổi bật.
- Tiêu đề, mô tả, label, helper text và dữ liệu có hierarchy nhất quán.
- Giữ icon SVG hiện có; không dùng emoji làm icon giao diện.

## Component patterns

### Page header

- Gồm eyebrow hoặc breadcrumb khi hữu ích, `h1`, mô tả và action area.
- CTA chính nằm bên phải trên desktop và có thể full-width trên mobile.

### Quick actions

- Chỉ chứa tác vụ có giá trị cao theo role.
- Mỗi action dùng động từ rõ ràng, icon phù hợp và mô tả ngắn khi cần.
- Không dùng nhiều CTA primary trong cùng một vùng.

### Cards và sections

- Metric card chỉ hiển thị dữ liệu thực có trong model.
- Interactive card có hover/focus rõ và toàn bộ mục tiêu click dễ hiểu.
- Nội dung đọc dài giới hạn chiều rộng; table và Chat được phép dùng vùng rộng hơn.

### Tables và toolbars

- Toolbar có thứ tự ổn định: search/filter bên trái, create/import/export bên phải.
- Header rõ, row height vừa phải, status badge semantic và action được gom gọn.
- Mobile cho phép horizontal scroll có container rõ hoặc stacked row nếu markup phù hợp.

### Forms

- Chia field theo nhóm logic; label luôn hiện rõ.
- Helper và validation nằm sát field liên quan.
- Primary submit rõ ràng; secondary/cancel có độ ưu tiên thấp hơn.
- Trạng thái disabled/loading không làm thay đổi bố cục đột ngột.

### Feedback states

- Empty state giải thích trạng thái và nêu bước tiếp theo.
- Error state thân thiện, đặt gần ngữ cảnh lỗi.
- Loading dùng skeleton cho content và spinner nhỏ cho action ngắn.
- Success, warning, danger và info dùng cả text/icon lẫn màu.

## Tối ưu theo khu vực

### Dashboard

- Hero chào mừng được thu gọn để dành không gian cho tác vụ.
- Quick actions theo role xuất hiện trước summary hoặc metric.
- Lecturer tiếp tục dùng dữ liệu `ManagedSubjects` hiện có.
- Không bổ sung dữ liệu backend chỉ để tạo dashboard đẹp hơn.

### Chat

- Ưu tiên message viewport và composer.
- Giảm chiều cao header và bộ lọc môn học.
- History panel dễ phân biệt nhưng không cạnh tranh với nội dung hội thoại.
- Composer dễ chạm trên mobile và luôn giữ action gửi rõ ràng.

### Documents và Subjects

- Search/filter/action dùng toolbar thống nhất.
- Metadata phụ có độ tương phản thấp hơn tên tài liệu hoặc môn học.
- Upload, processing, rejected, failed và empty state có hierarchy rõ.
- Form/chapter management tách khỏi danh sách dữ liệu bằng section rõ ràng.

### Research

- Research Questions ưu tiên toolbar và khả năng quét câu hỏi.
- Experiments ưu tiên status, cấu hình, metric và action chạy/xem kết quả.
- Chart giữ Chart.js và dùng palette semantic có label/legend rõ.

### Packages và Payments

- Pricing card làm rõ tên gói, giá, thời hạn, giới hạn và một CTA.
- Checkout ưu tiên order summary trước CTA thanh toán.
- Lịch sử và return page làm rõ trạng thái cùng bước tiếp theo.
- Màn hình admin dùng table/toolbar/status chung với các module quản trị khác.

### Admin

- Dashboard ưu tiên lối tắt tác vụ theo domain trước metric phụ.
- Users, packages, payments và chunking dùng chung page header, toolbar và content surface.
- Destructive action có confirmation hiện có, danger style và mô tả đối tượng bị tác động.

## Responsive và accessibility

- Kiểm tra ở 320px, 768px, 1024px và 1440px.
- 320–767px: một cột, drawer navigation, action quan trọng dễ chạm và không có overflow ngoài ý muốn.
- 768–1023px: grid tối đa hai cột; toolbar wrap có chủ đích.
- Từ 1024px: sidebar cố định; task-first dashboard dùng vùng nội dung linh hoạt.
- Từ 1440px: giới hạn reading width, ngoại trừ Chat và data tables.
- Focus ring hiện rõ cho mọi interactive element.
- Icon-only button có accessible name.
- Contrast đạt WCAG 2.1 AA; trạng thái không truyền đạt chỉ bằng màu.
- Thứ tự heading và landmark hợp lý.

## Phạm vi kỹ thuật

Được phép chỉnh:

- Razor markup trong `Pages`.
- CSS trong `wwwroot/css`.
- JavaScript UI nhỏ trong `wwwroot/js/site.js` khi cần cho bố cục hoặc accessibility.
- SVG sprite và shared partial phục vụ hiển thị.

Không được chỉnh:

- Service, repository, entity và database schema.
- Endpoint, authorization policy hoặc role mapping.
- Luồng RAG, streaming, embedding và thanh toán.
- Framework frontend hoặc thư viện nghiệp vụ mới.

## Trình tự triển khai

1. Rà soát và tinh chỉnh token/component primitives.
2. Tối ưu app shell, sidebar và page header.
3. Tối ưu dashboard task-first cho bốn role.
4. Tối ưu Chat, Documents và Subjects.
5. Tối ưu Research, Packages và Admin.
6. Responsive, accessibility và state polish.
7. Build và browser verification theo role/breakpoint.

## Tiêu chí hoàn thành

- `dotnet build` thành công với 0 error.
- Login và các trang chính trả HTTP thành công.
- Sidebar, active state và CTA đúng cho cả bốn role.
- Mỗi dashboard ưu tiên đúng tác vụ của role và không hiển thị dữ liệu giả.
- Không có console error, asset 404 hoặc overflow ngoài ý muốn.
- Hoạt động tốt ở 320px, 768px, 1024px và 1440px.
- Keyboard focus, icon label và contrast đáp ứng yêu cầu accessibility.
- Toàn bộ thay đổi chỉ ảnh hưởng hình ảnh và bố cục, không thay đổi hành vi nghiệp vụ.

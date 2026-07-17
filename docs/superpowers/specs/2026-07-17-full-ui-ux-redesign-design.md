# VietnameseEduChat Full UI/UX Redesign

## Mục tiêu

Thiết kế lại toàn bộ giao diện VietnameseEduChat theo hướng “học thuật hiện đại”: sáng, rõ ràng, đáng tin cậy và phù hợp cho việc đọc nội dung dài. Bản redesign phải thống nhất giữa trải nghiệm của Student, Lecturer, Academic Admin và System Admin mà không thay đổi nghiệp vụ, quyền truy cập hoặc luồng dữ liệu hiện có.

## Nguyên tắc thiết kế

- Nội dung học tập và hành động chính phải nổi bật hơn phần trang trí.
- Dùng một design system chung thay vì style riêng lẻ theo từng trang.
- Màu sắc có ý nghĩa: teal cho hành động chính, navy cho điều hướng, màu semantic cho trạng thái.
- Không dùng emoji làm icon giao diện; dùng icon SVG nhất quán và có nhãn hỗ trợ truy cập.
- Mỗi trang chỉ có một tiêu đề chính, một CTA ưu tiên và hierarchy dễ quét.
- Đạt WCAG 2.1 AA cho độ tương phản, focus và thao tác bàn phím.
- Hoạt động tốt ở 320 px, 768 px, 1024 px và 1440 px.

## Ngôn ngữ hình ảnh

### Màu sắc

- App background: xanh-xám rất nhạt (`#F4F7F9` hoặc tương đương).
- Surface/card: trắng, dùng viền nhẹ thay cho shadow nặng.
- Navigation: navy đậm để tạo điểm neo và cảm giác học thuật.
- Primary action: teal có độ tương phản AA trên nền sáng và nền navy.
- Text chính: slate đậm; text phụ: slate trung tính.
- Success, warning, danger và info dùng cả màu, icon và nhãn chữ; không truyền đạt trạng thái chỉ bằng màu.

Giá trị màu cụ thể sẽ được khai báo dưới dạng CSS variables semantic như `--color-primary`, `--color-surface`, `--color-text-muted` để dễ chỉnh theme và không rải mã màu trong Razor markup.

### Typography và spacing

- Dùng Inter hiện có, bổ sung font fallback hệ thống.
- Thang chữ rõ ràng cho page title, section title, body, label và helper text.
- Spacing theo nhịp 4 px; các khoảng thường dùng là 4, 8, 12, 16, 24, 32 và 48 px.
- Border radius có hierarchy: nhỏ cho input/badge, vừa cho card, lớn chỉ dùng ở vùng nổi bật.
- Motion ngắn 150–200 ms; tôn trọng `prefers-reduced-motion`.

## Kiến trúc giao diện

### App shell

Sau đăng nhập, giao diện dùng sidebar navy bên trái và vùng nội dung sáng bên phải.

- Sidebar chứa logo, nhóm điều hướng theo vai trò và trạng thái active rõ ràng.
- User menu và đăng xuất nằm cuối sidebar, tránh cạnh tranh với navigation chính.
- Desktop: sidebar cố định, nội dung có chiều rộng linh hoạt.
- Tablet/mobile: sidebar trở thành off-canvas drawer, có nút mở/đóng dễ thấy và focus được quản lý đúng.
- Login và các trang không xác thực dùng auth layout riêng, không hiển thị navigation rỗng.

### Thành phần dùng chung

- Page header: eyebrow/breadcrumb tùy ngữ cảnh, tiêu đề, mô tả và vùng action.
- Button: primary, secondary, subtle, danger; đủ trạng thái hover, focus, disabled và loading.
- Form: label rõ, helper/error gần field, input height nhất quán.
- Card: default, interactive và metric card; không dùng mọi nội dung như một card giống nhau.
- Table: header rõ, hover nhẹ, action menu gọn, hỗ trợ cuộn ngang trên mobile.
- Badge/status: semantic, có text và icon khi cần.
- Alert/toast: success, warning, error, info; nội dung ngắn và action rõ.
- Empty state: giải thích trạng thái và cung cấp hành động tiếp theo.
- Modal/dropdown: focus, keyboard và backdrop đúng chuẩn Bootstrap.
- Loading: skeleton cho khối nội dung; spinner chỉ dùng cho hành động ngắn.

## Thiết kế theo màn hình

### Đăng nhập

- Auth layout hai vùng trên desktop: thông điệp sản phẩm ngắn và form đăng nhập tập trung.
- Mobile chuyển thành một cột.
- Hiển thị/ẩn mật khẩu, lỗi validation rõ và không phụ thuộc màu.
- Tài khoản demo được trình bày như thông tin hỗ trợ, không lấn CTA đăng nhập.

### Dashboard người dùng

- Hero nhỏ gọn chào người dùng và hiển thị role theo cách dễ hiểu.
- Quick actions ưu tiên “Hỏi đáp với AI” và tác vụ chính theo role.
- Metric/summary chỉ hiển thị dữ liệu thực có trong model; không tạo số liệu giả.
- Lecturer giữ vùng môn học được phân công nhưng chuyển sang card dễ quét và CTA rõ.
- Các role admin có lối tắt đến quản trị, research và thanh toán theo quyền hiện có.

### Chat

- Bố cục ba vùng có hierarchy rõ: lịch sử hội thoại, vùng nội dung, composer.
- Sidebar hội thoại có tiêu đề, tạo mới, trạng thái rỗng và delete action dễ khám phá nhưng không gây nhiễu.
- Header chat và bộ lọc môn học gọn, không chiếm quá nhiều chiều cao.
- Message AI có độ rộng đọc hợp lý, typography markdown tốt và phân biệt rõ với message người dùng.
- Nguồn tài liệu hiển thị thành disclosure/cards có tên tài liệu, trang, độ liên quan và excerpt.
- Composer bám đáy vùng chat, hỗ trợ Enter gửi và Shift+Enter xuống dòng; trạng thái streaming/disabled được thông báo.
- Mobile cho phép mở danh sách hội thoại dạng drawer thay vì xếp chồng thành khối cao.

### Tài liệu

- Danh sách tài liệu có page header, search/filter toolbar, status badge và action rõ theo quyền.
- Upload dùng vùng chọn file dễ hiểu, mô tả định dạng/kích thước và trạng thái xử lý.
- Details nhóm metadata, trạng thái, nội dung/trích xuất và action theo thứ tự ưu tiên.
- Empty, processing, rejected và failed states có lời giải thích cùng bước tiếp theo.

### Môn học và chương

- Danh sách môn học ưu tiên tên, mô tả, số chương và giảng viên.
- Quản lý chương được tổ chức theo section/list có thứ tự, tránh form và dữ liệu trộn lẫn.
- Các thao tác thêm/sửa/xóa dùng pattern thống nhất và xác nhận khi có rủi ro mất dữ liệu.

### Gói dịch vụ và thanh toán

- Pricing cards thể hiện tên gói, giá, thời hạn, giới hạn và một CTA rõ.
- Gói hiện tại hoặc lựa chọn đề xuất có nhãn chữ, không chỉ khác màu.
- Checkout tóm tắt đơn hàng trước CTA thanh toán.
- History và VNPay return hiển thị trạng thái, mã giao dịch và bước tiếp theo dễ hiểu.
- Trang admin Packages/Payments dùng table, filter và status components chung.

### Research Questions và Experiments

- Các form tạo/import được tách khỏi vùng dữ liệu chính bằng drawer/modal hoặc section rõ ràng, tùy cấu trúc markup hiện có.
- Toolbar chứa search, filter, import/export và create action theo mức ưu tiên.
- Experiment cards/table làm nổi bật status, cấu hình model/chunking, metrics và action chạy/xem kết quả.
- Chart giữ Chart.js hiện có nhưng dùng palette semantic mới, legend rõ và không phụ thuộc màu duy nhất.
- Nội dung dày dữ liệu được ưu tiên khả năng quét và responsive hơn trang trí.

### System/Academic Admin

- Dashboard admin dùng metric cards có icon SVG và nhóm tác vụ theo domain.
- User management, chunking config, packages và payments dùng cùng pattern page header + toolbar + content surface.
- Destructive actions dùng danger styling, confirmation và mô tả đối tượng bị tác động.
- Import user hiển thị yêu cầu file, validation và kết quả import dễ đọc.

### Error, Privacy và trạng thái phụ

- Error page có thông điệp thân thiện, mã lỗi nếu hữu ích và đường quay lại an toàn.
- Privacy dùng content layout có chiều rộng đọc phù hợp.
- Footer tối giản, không chiếm không gian trong các màn hình tác vụ như Chat.

## Responsive behavior

- 320–767 px: một cột; app sidebar là drawer; table cuộn ngang hoặc đổi sang stacked rows khi phù hợp; CTA chính full-width khi cần.
- 768–1023 px: sidebar thu gọn hoặc drawer; grid 2 cột; toolbar có thể wrap.
- Từ 1024 px: sidebar cố định; dashboard grid 3–4 cột tùy nội dung; chat dùng hai vùng chính.
- Từ 1440 px: giới hạn chiều rộng đọc cho page content, nhưng các trang table/chat được phép rộng hơn.

## Accessibility

- Giữ thứ tự heading hợp lý và landmark `header`, `nav`, `main`, `aside`.
- Mọi input có label liên kết bằng `for`/`id` hoặc tag helper tương đương.
- Icon-only button có `aria-label` và tooltip hỗ trợ, không dùng title làm nhãn duy nhất.
- Focus ring rõ trên mọi interactive element.
- Off-canvas, dropdown và modal thao tác được bằng bàn phím.
- Trạng thái loading/error/streaming dùng `aria-live` khi phù hợp.
- Kiểm tra contrast tối thiểu 4.5:1 cho chữ thường và 3:1 cho chữ lớn/UI components.

## Phạm vi kỹ thuật

- Giữ ASP.NET Core Razor Pages, Bootstrap, jQuery, SignalR, Marked và Chart.js hiện có.
- Không thay đổi database, service, repository, authentication hoặc authorization trừ khi cần thêm dữ liệu hiển thị đã có sẵn.
- Tổ chức lại `_Layout.cshtml`, CSS toàn cục và markup Razor để hỗ trợ app shell mới.
- Loại bỏ `dark-theme.css` khỏi runtime sau khi các rule cần thiết đã được hợp nhất hoặc thay thế.
- Dùng SVG/icon library nhẹ được lưu cục bộ hoặc inline partials; không phụ thuộc emoji và không bắt buộc tải icon từ CDN.
- Hạn chế inline style; chuyển style lặp lại vào class/component dùng chung.
- Không thêm framework frontend mới.

## Trình tự triển khai dự kiến

1. Design tokens, reset và component primitives.
2. App shell, responsive navigation và auth layout.
3. Login, Dashboard và Chat.
4. Documents, Subjects và Packages.
5. Research modules và Admin modules.
6. Error/Privacy, responsive polish và accessibility pass.
7. Build, smoke test theo role và browser verification ở các breakpoint.

## Kiểm thử và tiêu chí hoàn thành

- `dotnet build` thành công, không có lỗi Razor/C#.
- App khởi động và trang Login trả HTTP 200.
- Đăng nhập và smoke test navigation cho bốn role mặc định.
- Các trang chính không có lỗi console hoặc asset 404.
- Kiểm tra visual ở 320, 768, 1024 và 1440 px.
- Keyboard test cho navigation, forms, dropdown/modal và composer chat.
- Không có chữ chìm, overflow ngoài ý muốn hoặc action bị che.
- Tất cả trang trong thư mục `Pages` dùng design system mới hoặc content layout có chủ đích.

## Ngoài phạm vi

- Thay đổi nghiệp vụ RAG, embedding hoặc streaming.
- Thay đổi cơ chế thanh toán VNPay.
- Viết lại frontend bằng React/Vue hoặc framework khác.
- Tạo mobile app native.
- Thay đổi schema database hoặc quyền của các role.

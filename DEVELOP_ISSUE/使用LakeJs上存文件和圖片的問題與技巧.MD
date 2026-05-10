# 🟩使用LakeJs上存文件和圖片的問題與技巧



## 🔴定義 LakeJS

```
 <!--LakeJS-->
 <link href="~/lib/lakelib/dist/lake.min.css" rel="stylesheet" />
 <script src="~/lib/lakelib/dist/lake.min.js"></script>
```



## 🔴image, media, file 定義出現http404問題

> 先預設的上存文件的URL

```
const mainComId = "@ViewBag.MainComId";
const authenticationCode = "@ViewBag.AuthenticationCode";
// ImgUrl
const uploadImgUrl =  `/Home/UploadImg?mainComId=${mainComId}&token=${authenticationCode}`;
// Media Url
const uploadMediaUrl =  `/Home/UploadMedia?mainComId=${mainComId}&token=${authenticationCode}`;
// File Url
const uploadFileUrl =  `/Home/UploadFile?mainComId=${mainComId}&token=${authenticationCode}`;
```



## 🔴Cannot find the url field.

```
後端代碼必須的json格式符合 Lakejs
//lakejs  返回的格式要求 
object logData = new { result = true, msg = "picture uploaded successfully", url = fileUrl };
```



## 🔴最終的正確代碼

```
@section Scripts {
    <script>
         
        const mainComId = "@ViewBag.MainComId";
        const authenticationCode = "@ViewBag.AuthenticationCode";
        const uploadImgUrl =  `/Home/UploadImg?mainComId=${mainComId}&token=${authenticationCode}`;
        console.log('uploadImgUrl:' + uploadImgUrl);
        const uploadMediaUrl =  `/Home/UploadMedia?mainComId=${mainComId}&token=${authenticationCode}`;
        console.log('uploadMediaUrl:' + uploadMediaUrl);
        const uploadFileUrl =  `/Home/UploadFile?mainComId=${mainComId}&token=${authenticationCode}`;
        console.log('uploadFileUrl:' + uploadFileUrl);
        
        // MainComId验证 - 确保在上传图片前已经填写了公司ID
        if (!mainComId) {
            console.error('mainComId is required for image upload'); 
            showResult('Input MainComId', 'danger'); 
        }

        let lakeEditor = null;

        document.addEventListener('DOMContentLoaded', function () {
             
            const { Editor, Toolbar } = Lake;
             
            // 工具栏配置保持不变 'image','media', 'file',
            const toolbarItems = [
                'undo', 'redo', '|', 'heading', 'fontFamily', 'fontSize', '|',
                'formatPainter', 'removeFormat', 'bold', 'italic', 'underline',
                'strikethrough', 'superscript', 'subscript', 'code', 'moreStyle', '|',
                'fontColor', 'highlight', '|', 'list', 'numberedList', 'bulletedList',
                'checklist', 'table', '-', 'align', 'alignLeft', 'alignCenter', 'alignRight',
                'alignJustify', '|', 'indent', 'increaseIndent', 'decreaseIndent', '|',
                'link', 'image', 'codeBlock', 'equation', 'video',
                'twitter', 'blockQuote', 'paragraph', 'hr', '|', 'selectAll'
            ];

            const toolbar = new Toolbar({
                root: '.my-toolbar',
                items: toolbarItems,
                placement: 'top',
                fontFamily: {
                    defaultValue: '微软雅黑',
                    menuItems: [
                        { value: 'Arial', text: 'Arial' },
                        { value: '微软雅黑', text: '微软雅黑' },
                        { value: 'Times New Roman', text: 'Times New Roman' }
                    ]
                }
            });

            lakeEditor = new Editor({
                root: '.my-content',
                toolbar: toolbar,
                image: {
                    requestMethod: 'POST',
                    requestAction: uploadImgUrl,
                    requestTypes: ['image/gif', 'image/jpeg', 'image/png'],
                },
                value: '<p>在這裡輸入郵件內容...</p>',
            });

            lakeEditor.render();
            const contentDiv = document.querySelector('.my-content');
            if (contentDiv) contentDiv.style.minHeight = '300px';
        });

        // 获取提交按钮元素
        const submitBtn = document.querySelector('#mailForm button[type="submit"]');
        let originalBtnHtml = submitBtn ? submitBtn.innerHTML : '';

        // 表单提交处理
        document.getElementById('mailForm').addEventListener('submit', async function (e) {
            e.preventDefault();
             
            // 1. 同步编辑器内容到隐藏字段
            const emailBody = lakeEditor.getValue();
            document.getElementById('bodyHtml').value = emailBody;

            // 2. 收集表单数据
            const formData = new FormData(this);
            console.log(formData.get('mainComId'));
            const data = {
                mainComId: formData.get('mainComId'),
                mailto: formData.get('mailto'),
                subject: formData.get('subject'),
                emailContent: formData.get('bodyHtml'),
                callbackUrlEncode: formData.get('callbackUrlEncode')
            };

             // 3. MainComId验证
            if (!data.mainComId ) {
                showResult('公司ID( MainComID) 是必須填入的', 'danger');
                return;
            }

            // 4. 简单验证
            if (!data.mailto || !data.subject) {
                showResult('請填寫收件人和主旨', 'danger');
                return;
            }
            //在這裡輸入郵件內容...
            if (!data.emailContent || data.emailContent === '<p>在這裡輸入郵件內容...</p>') {
                showResult('請填寫郵件正文', 'warning');
                return;
            }

            // 4. 禁用按钮，显示加载状态
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> 發送中...';
            }

            try {
                 
                const authenticationCode = document.getElementById('authenticationCode').value;
                // 注意：标准 Authorization 头格式为 "Bearer token"
                const authHeader = `Bearer ${authenticationCode}`;

                const response = await fetch('/Home/EmailEditor', {
                    method: 'POST',
                        headers: {
                        'Content-Type': 'application/json',
                        'Authorization': authHeader   
                    },
                    body: JSON.stringify(data)
                });
                const result = await response.json();

                // 根据后端 ResponseModalX 结构，meta 中的字段是小写 success, message
                if (response.ok && result.meta && result.meta.success === true) {
                    showResult('✅ 郵件發送成功！', 'success');
                    // 清空表单（注意收件人元素 id 是 mailto，不是 toEmail）
                    document.getElementById('mailto').value = '';
                    document.getElementById('subject').value = '';
                    document.getElementById('callbackUrlEncode').value = '';
                    document.getElementById('mainComId').value = '';
                    lakeEditor.setValue('<p>在這裡輸入郵件內容...</p>');
                } else {
                    const errorMsg = result.meta?.message || '發送失敗';
                    showResult('❌ ' + errorMsg, 'danger');
                }
            } catch (error) {
                console.error('提交出錯：', error);
                showResult('❌ 網路錯誤，請稍後重試', 'danger');
            } finally {
                // 5. 无论成功或失败，恢复按钮状态
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnHtml;
                }
            }
        });

        function showResult(message, type) {
            const resultDiv = document.getElementById('resultMessage');
            const alertClass = `alert alert-${type === 'danger' ? 'danger' : (type === 'success' ? 'success' : 'warning')}`;
            resultDiv.innerHTML = `<div class="${alertClass} alert-dismissible fade show" role="alert">
                                        ${message}
                                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                                    </div>`;
            // 10秒后自动清除消息
            setTimeout(() => resultDiv.innerHTML = '', 10000);
        }

        // 通用上传函数(not work)
        async function upload(file, url, insertFn) { 
            const authHeader = `Bearer ${document.getElementById('authenticationCode').value}`;
            const formData = new FormData();
            formData.append('mainComId', mainComId);
            formData.append('file', file);
            //alert('正在上传文件，请稍候...');
            try {
                const response = await fetch(url, {
                    method: 'POST',
                    body: formData,
                    headers: { 'Authorization': authHeader }
                });
                const result = await response.json();
                if (result.code === 0) {
                    insertFn(result.data.url);
                } else {
                    console.error('Upload failed:', result.msg);
                }
            } catch (error) {
                console.error('Upload error:', error);
            }
        } 
    </script>
}
```


document.addEventListener('DOMContentLoaded', function () {
    console.log('CreateUser View loaded - Debug version');

    // Debug: Kiểm tra tất cả các elements có tồn tại không
    console.log('=== DEBUGGING ELEMENTS ===');

    const passwordTypeRadios = document.querySelectorAll('input[name="PasswordType"]');
    console.log('Password type radios found:', passwordTypeRadios.length);

    const customPasswordFields = document.getElementById('customPasswordFields');
    console.log('Custom password fields:', customPasswordFields);

    const customPasswordInput = document.querySelector('input[name="CustomPassword"]');
    console.log('Custom password input:', customPasswordInput);

    const confirmPasswordInput = document.querySelector('input[name="ConfirmPassword"]');
    console.log('Confirm password input:', confirmPasswordInput);

    const emailInput = document.querySelector('input[name="Email"]');
    console.log('Email input:', emailInput);

    const phoneInput = document.querySelector('input[name="PhoneNumber"]');
    console.log('Phone input:', phoneInput);

    // Thử tìm với các selector khác nếu không tìm thấy
    if (!emailInput) {
        const emailInputAlt = document.querySelector('#Email');
        console.log('Email input (by ID):', emailInputAlt);
    }

    if (!phoneInput) {
        const phoneInputAlt = document.querySelector('#PhoneNumber');
        console.log('Phone input (by ID):', phoneInputAlt);
    }

    if (!customPasswordInput) {
        const customPasswordInputAlt = document.querySelector('#CustomPassword');
        console.log('Custom password input (by ID):', customPasswordInputAlt);
    }

    // Liệt kê tất cả các input elements để debug
    const allInputs = document.querySelectorAll('input');
    console.log('All inputs found:');
    allInputs.forEach((input, index) => {
        console.log(`Input ${index}:`, {
            name: input.name,
            id: input.id,
            type: input.type,
            tagName: input.tagName
        });
    });

    console.log('=== END DEBUGGING ===');

    // Sử dụng selector linh hoạt hơn
    const getEmailInput = () => {
        return document.querySelector('input[name="Email"]') ||
            document.querySelector('#Email') ||
            document.querySelector('input[type="email"]');
    };

    const getPhoneInput = () => {
        return document.querySelector('input[name="PhoneNumber"]') ||
            document.querySelector('#PhoneNumber') ||
            document.querySelector('input[name="Phone"]');
    };

    const getCustomPasswordInput = () => {
        return document.querySelector('input[name="CustomPassword"]') ||
            document.querySelector('#CustomPassword');
    };

    const getConfirmPasswordInput = () => {
        return document.querySelector('input[name="ConfirmPassword"]') ||
            document.querySelector('#ConfirmPassword');
    };

    const dobInput = document.querySelector('input[name="DateOfBirth"]');
    const roleSelector = document.getElementById("roleSelector");
    const codeBox = document.getElementById("autoCode");
    const label = document.getElementById("codeLabel");
    const boxContainer = document.getElementById("autoCodeContainer");
    const form = document.querySelector('form');

    function handlePasswordTypeChange() {
        const selectedType = document.querySelector('input[name="PasswordType"]:checked');
        console.log('Password type changed:', selectedType ? selectedType.value : 'none');

        if (selectedType && selectedType.value === '1') {
            showCustomPasswordFields();
        } else {
            hideCustomPasswordFields();
        }
    }

    function showCustomPasswordFields() {
        console.log('Showing custom password fields');
        if (customPasswordFields) {
            customPasswordFields.style.display = 'block';
        }

        const customPwdInput = getCustomPasswordInput();
        const confirmPwdInput = getConfirmPasswordInput();

        if (customPwdInput) {
            customPwdInput.required = true;
            customPwdInput.value = '';
            setTimeout(() => customPwdInput.focus(), 100);
        }

        if (confirmPwdInput) {
            confirmPwdInput.required = true;
            confirmPwdInput.value = '';
        }
    }

    function hideCustomPasswordFields() {
        console.log('Hiding custom password fields');
        if (customPasswordFields) {
            customPasswordFields.style.display = 'none';
        }

        const customPwdInput = getCustomPasswordInput();
        const confirmPwdInput = getConfirmPasswordInput();

        if (customPwdInput) customPwdInput.required = false;
        if (confirmPwdInput) confirmPwdInput.required = false;

        // Tự động tạo mật khẩu từ email và số điện thoại
        updateGeneratedPassword();
    }

    function generatePasswordFromEmail(email, phone) {
        console.log('=== GENERATING PASSWORD ===');
        console.log('Input email:', email);
        console.log('Input phone:', phone);

        if (!email || email.trim() === '') {
            console.log('Email is empty, returning default');
            return "User123";
        }

        const cleanEmail = email.trim();
        const atIndex = cleanEmail.indexOf("@");

        if (atIndex <= 0) {
            console.log('Invalid email format, returning default');
            return "User123";
        }

        let localPart = cleanEmail.substring(0, atIndex);
        console.log('Local part before processing:', localPart);

        // Viết hoa chữ cái đầu tiên
        localPart = localPart.charAt(0).toUpperCase() + localPart.slice(1);
        console.log('Local part after capitalize:', localPart);

        // Luôn thêm 3 số cuối SDT nếu có, ngược lại thêm "123"
        if (phone && phone.trim().length >= 3) {
            const cleanPhone = phone.trim();
            const lastThree = cleanPhone.slice(-3);
            localPart += lastThree;
            console.log('Added phone digits:', lastThree);
        } else {
            localPart += "123";
            console.log('Added default digits: 123');
        }

        console.log('Local part after adding digits:', localPart);

        // Đảm bảo chứa ít nhất 1 số
        if (!/\d/.test(localPart)) {
            localPart += "1";
            console.log('Added digit for requirement');
        }

        // Đảm bảo chứa ít nhất 1 chữ hoa
        if (!/[A-Z]/.test(localPart)) {
            localPart = "A" + localPart;
            console.log('Added uppercase letter');
        }

        // Đảm bảo độ dài tối thiểu (Identity yêu cầu mặc định: ≥6)
        if (localPart.length < 6) {
            localPart += "Xyz123";
            console.log('Extended for minimum length');
        }

        console.log('Final generated password:', localPart);
        console.log('=== END GENERATING PASSWORD ===');
        return localPart;
    }

    function updateGeneratedPassword() {
        console.log('=== UPDATING GENERATED PASSWORD ===');

        const selectedType = document.querySelector('input[name="PasswordType"]:checked');
        const emailInput = getEmailInput();
        const phoneInput = getPhoneInput();
        const customPwdInput = getCustomPasswordInput();
        const confirmPwdInput = getConfirmPasswordInput();

        console.log('Selected password type:', selectedType ? selectedType.value : 'none');
        console.log('Email input element:', emailInput);
        console.log('Phone input element:', phoneInput);
        console.log('Custom password input element:', customPwdInput);
        console.log('Confirm password input element:', confirmPwdInput);

        if (selectedType && selectedType.value === '0') {
            const email = emailInput ? emailInput.value : '';
            const phone = phoneInput ? phoneInput.value : '';

            console.log('Current email value:', email);
            console.log('Current phone value:', phone);

            if (email && email.trim() !== '') {
                const generated = generatePasswordFromEmail(email, phone);

                if (customPwdInput) {
                    customPwdInput.value = generated;
                    console.log('Set custom password input value:', generated);
                } else {
                    console.log('Custom password input not found!');
                }

                if (confirmPwdInput) {
                    confirmPwdInput.value = generated;
                    console.log('Set confirm password input value:', generated);
                } else {
                    console.log('Confirm password input not found!');
                }
            } else {
                console.log('Email is empty, not generating password');
            }
        }

        console.log('=== END UPDATING GENERATED PASSWORD ===');
    }

    function handleEmailChange() {
        console.log('Email changed event triggered');
        updateGeneratedPassword();
    }

    function handlePhoneChange() {
        console.log('Phone changed event triggered');
        updateGeneratedPassword();
    }

    function generateUserCode(role) {
        const now = new Date();
        const year = now.getFullYear().toString().slice(-2);
        const random = Math.floor(Math.random() * 9000) + 1000;

        switch (role) {
            case "Student":
                return { label: "Mã học sinh", value: "SD" + year + random };
            case "Teacher":
                return { label: "Mã nhân viên", value: "TC" + year + random };
            case "Admin":
                return { label: "Mã nhân viên", value: "AD" + year + random };
            default:
                return null;
        }
    }

    function validateAge() {
        if (!dobInput || !dobInput.value) return true;

        const dob = new Date(dobInput.value);
        const today = new Date();
        let age = today.getFullYear() - dob.getFullYear();
        const m = today.getMonth() - dob.getMonth();
        if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) {
            age--;
        }

        if (age < 5) {
            alert("Người dùng phải ít nhất 5 tuổi để đăng ký.");
            dobInput.focus();
            return false;
        }
        return true;
    }

    function validateForm(e) {
        console.log('Form validation triggered');

        const selectedType = document.querySelector('input[name="PasswordType"]:checked');
        const customPwdInput = getCustomPasswordInput();
        const confirmPwdInput = getConfirmPasswordInput();
        const emailInput = getEmailInput();
        const phoneInput = getPhoneInput();

        if (selectedType && selectedType.value === '1') {
            // Kiểm tra mật khẩu tùy chỉnh
            const customPassword = customPwdInput ? customPwdInput.value.trim() : '';
            const confirmPassword = confirmPwdInput ? confirmPwdInput.value.trim() : '';

            if (!customPassword) {
                alert('Vui lòng nhập mật khẩu tùy chỉnh');
                if (customPwdInput) customPwdInput.focus();
                e.preventDefault();
                return false;
            }

            if (customPassword.length < 6) {
                alert('Mật khẩu phải có ít nhất 6 ký tự');
                if (customPwdInput) customPwdInput.focus();
                e.preventDefault();
                return false;
            }

            if (customPassword !== confirmPassword) {
                alert('Mật khẩu xác nhận không khớp');
                if (confirmPwdInput) confirmPwdInput.focus();
                e.preventDefault();
                return false;
            }
        }

        if (selectedType && selectedType.value === '0') {
            // Sử dụng email làm mật khẩu
            const email = emailInput ? emailInput.value.trim() : '';
            if (!email) {
                alert('Vui lòng nhập email trước khi sử dụng làm mật khẩu');
                if (emailInput) emailInput.focus();
                e.preventDefault();
                return false;
            }

            // Đảm bảo mật khẩu được tạo đúng trước khi submit
            const phone = phoneInput ? phoneInput.value.trim() : '';
            const generated = generatePasswordFromEmail(email, phone);

            if (customPwdInput) customPwdInput.value = generated;
            if (confirmPwdInput) confirmPwdInput.value = generated;

            console.log('Final password before submit:', generated);
        }

        if (!validateAge()) {
            e.preventDefault();
            return false;
        }

        console.log('Form validation passed');
    }

    // Event listeners với kiểm tra tồn tại
    passwordTypeRadios.forEach(radio => radio.addEventListener('change', handlePasswordTypeChange));

    const emailInput = getEmailInput();
    if (emailInput) {
        emailInput.addEventListener('input', handleEmailChange);
        emailInput.addEventListener('blur', handleEmailChange);
        console.log('Email event listeners attached');
    } else {
        console.log('Could not find email input for event listeners');
    }

    const phoneInput = getPhoneInput();
    if (phoneInput) {
        phoneInput.addEventListener('input', handlePhoneChange);
        phoneInput.addEventListener('blur', handlePhoneChange);
        console.log('Phone event listeners attached');
    } else {
        console.log('Could not find phone input for event listeners');
    }

    // Khởi tạo ban đầu
    handlePasswordTypeChange();

    if (form) {
        form.addEventListener('submit', validateForm);
        console.log('Form submit listener attached');
    }

    if (roleSelector) {
        roleSelector.addEventListener("change", function () {
            const role = this.value;
            const result = generateUserCode(role);
            if (result) {
                if (boxContainer) boxContainer.style.display = "block";
                if (label) label.textContent = result.label;
                if (codeBox) codeBox.value = result.value;
            } else {
                if (boxContainer) boxContainer.style.display = "none";
            }
        });
    }

    // Test password generation ngay khi load trang
    setTimeout(() => {
        console.log('=== TESTING PASSWORD GENERATION ===');
        const testEmail = 'quan@gmail.com';
        const testPhone = '0862409720';
        const testResult = generatePasswordFromEmail(testEmail, testPhone);
        console.log('Test result for quan@gmail.com + 0862409720:', testResult);
        console.log('Expected: Quan720');
        console.log('=== END TEST ===');
    }, 1000);
});
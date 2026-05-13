import React, { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

// Простейшие правила под ASP.NET Identity по умолчанию:
// длина ≥ 6, есть строчные, прописные, цифра, спецсимвол
const passwordChecks = [
    { id: "len", test: (p) => p.length >= 6, label: "Минимум 6 символов" },
    { id: "lower", test: (p) => /[a-z]/.test(p), label: "Строчная буква" },
    { id: "upper", test: (p) => /[A-Z]/.test(p), label: "Заглавная буква" },
    { id: "digit", test: (p) => /\d/.test(p), label: "Цифра" },
    { id: "special", test: (p) => /[^A-Za-z0-9]/.test(p), label: "Спецсимвол" },
];

function calcStrength(pwd) {
    // простое суммирование выполненных проверок
    const passed = passwordChecks.reduce((n, c) => n + (c.test(pwd) ? 1 : 0), 0);
    // нормируем к 0..100
    return Math.round((passed / passwordChecks.length) * 100);
}

const emailRegex =
    /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/i;

// Лояльная проверка для международного номера: + и 7-15 цифр
const phoneRegex =
    /^\+?\d{7,15}$/;

export default function Register() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [showPwd, setShowPwd] = useState(false);
    const [confirm, setConfirm] = useState("");
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [phoneNumber, setPhoneNumber] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const [serverError, setServerError] = useState("");

    const { login } = useAuth();
    const navigate = useNavigate();

    const strength = useMemo(() => calcStrength(password), [password]);
    const checks = useMemo(
        () => passwordChecks.map(c => ({ ...c, ok: c.test(password) })),
        [password]
    );

    const emailValid = emailRegex.test(email);
    const phoneValid = !phoneNumber || phoneRegex.test(phoneNumber); // телефон опционален? оставим мягко
    const passwordsMatch = password === confirm;
    const passwordValid = checks.every(c => c.ok);

    const canSubmit =
        firstName.trim() &&
        lastName.trim() &&
        emailValid &&
        phoneValid &&
        passwordValid &&
        passwordsMatch &&
        !submitting;

    const handleRegister = async (e) => {
        e.preventDefault();
        setServerError("");

        if (!canSubmit) return;

        try {
            setSubmitting(true);

            // ⚠️ ваши реальные эндпоинты: /auth/register и /auth/login
            const res = await fetch(`${import.meta.env.VITE_IDENTITY_URL}/auth/register`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    userName: email, // используем email как логин
                    email,
                    password,
                    firstName,
                    lastName,
                    phoneNumber
                }),
            });

            if (!res.ok) {
                const msg = await safeReadError(res);
                setServerError(msg || "Ошибка регистрации");
                return;
            }

            // авто-вход
            const loginRes = await fetch(`${import.meta.env.VITE_IDENTITY_URL}/auth/login`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password }),
            });

            if (loginRes.ok) {
                const data = await loginRes.json();
                login(data.token);
                navigate("/");
            } else {
                const msg = await safeReadError(loginRes);
                setServerError(msg || "Регистрация прошла, но вход не удался");
                navigate("/login");
            }
        } catch (err) {
            setServerError("Сеть недоступна или сервер не отвечает.");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-[color:var(--color-brand-light)] to-[color:var(--color-brand-dark)]">
            <div className="w-full max-w-md bg-white shadow-xl rounded-2xl p-8">
                <h2 className="text-2xl font-bold text-center text-gray-800 mb-6">
                    Регистрация
                </h2>

                {serverError && (
                    <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                        {serverError}
                    </div>
                )}

                <form onSubmit={handleRegister} className="space-y-4">
                    {/* Имя */}
                    <div>
                        <input
                            type="text"
                            placeholder="Имя"
                            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                            value={firstName}
                            onChange={(e) => setFirstName(e.target.value)}
                            required
                        />
                        <p className="mt-1 text-xs text-gray-500">
                            Укажите ваше имя (как в профиле).
                        </p>
                    </div>

                    {/* Фамилия */}
                    <div>
                        <input
                            type="text"
                            placeholder="Фамилия"
                            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                            value={lastName}
                            onChange={(e) => setLastName(e.target.value)}
                            required
                        />
                        <p className="mt-1 text-xs text-gray-500">
                            Укажите вашу фамилию.
                        </p>
                    </div>

                    {/* Телефон */}
                    <div>
                        <input
                            type="tel"
                            placeholder="Телефон (например, +79123456789)"
                            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:outline-none ${phoneNumber && !phoneValid
                                    ? "border-red-400 focus:ring-red-300"
                                    : "focus:ring-[color:var(--color-brand)]"
                                }`}
                            value={phoneNumber}
                            onChange={(e) => setPhoneNumber(e.target.value.trim())}
                        />
                        <p className={`mt-1 text-xs ${phoneNumber && !phoneValid ? "text-red-600" : "text-gray-500"
                            }`}>
                            Допустим формат: «+» и 7–15 цифр. Пример: +79123456789.
                        </p>
                    </div>

                    {/* Email */}
                    <div>
                        <input
                            type="email"
                            placeholder="Email"
                            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:outline-none ${email && !emailValid
                                    ? "border-red-400 focus:ring-red-300"
                                    : "focus:ring-[color:var(--color-brand)]"
                                }`}
                            value={email}
                            onChange={(e) => setEmail(e.target.value.trim())}
                            required
                        />
                        <p className={`mt-1 text-xs ${email && !emailValid ? "text-red-600" : "text-gray-500"
                            }`}>
                            Введите действительный email. Он будет вашим логином.
                        </p>
                    </div>

                    {/* Пароль */}
                    <div>
                        <div className="relative">
                            <input
                                type={showPwd ? "text" : "password"}
                                placeholder="Пароль"
                                className="w-full pr-12 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[color:var(--color-brand)] focus:outline-none"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                                autoComplete="new-password"
                            />
                            <button
                                type="button"
                                onClick={() => setShowPwd(s => !s)}
                                className="absolute right-3 top-1/2 -translate-y-1/2 text-sm text-gray-500 hover:text-gray-700"
                                aria-label={showPwd ? "Скрыть пароль" : "Показать пароль"}
                            >
                                {showPwd ? "Скрыть" : "Показать"}
                            </button>
                        </div>

                        {/* Индикатор силы */}
                        <div className="mt-2">
                            <div className="h-2 w-full rounded bg-gray-200 overflow-hidden">
                                <div
                                    className={`h-2 transition-all ${strength < 40
                                            ? "bg-red-500"
                                            : strength < 80
                                                ? "bg-yellow-500"
                                                : "bg-green-500"
                                        }`}
                                    style={{ width: `${strength}%` }}
                                />
                            </div>
                            <p className="mt-1 text-xs text-gray-500">
                                Надёжность пароля: {strength}%
                            </p>
                        </div>

                        {/* Чек-лист требований */}
                        <ul className="mt-2 grid grid-cols-2 gap-x-4 gap-y-1 text-xs">
                            {checks.map(c => (
                                <li key={c.id} className={c.ok ? "text-green-600" : "text-gray-500"}>
                                    {c.ok ? "✓" : "•"} {c.label}
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Подтверждение пароля */}
                    <div>
                        <input
                            type="password"
                            placeholder="Подтвердите пароль"
                            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:outline-none ${confirm && !passwordsMatch
                                    ? "border-red-400 focus:ring-red-300"
                                    : "focus:ring-[color:var(--color-brand)]"
                                }`}
                            value={confirm}
                            onChange={(e) => setConfirm(e.target.value)}
                            required
                            autoComplete="new-password"
                        />
                        <p className={`mt-1 text-xs ${confirm && !passwordsMatch ? "text-red-600" : "text-gray-500"
                            }`}>
                            {confirm && !passwordsMatch
                                ? "Пароли не совпадают."
                                : "Повторите пароль для проверки."}
                        </p>
                    </div>

                    <button
                        type="submit"
                        disabled={!canSubmit}
                        className={`w-full text-white font-semibold py-2 rounded-lg transition ${canSubmit
                                ? "bg-[color:var(--color-brand)] hover:bg-[color:var(--color-brand-dark)]"
                                : "bg-gray-300 cursor-not-allowed"
                            }`}
                    >
                        {submitting ? "Регистрация..." : "Зарегистрироваться"}
                    </button>
                </form>
            </div>
        </div>
    );
}

// аккуратно читаем тело ошибки с бэка (Identity часто шлёт массив сообщений)
async function safeReadError(res) {
    try {
        const ct = res.headers.get("content-type") || "";
        if (ct.includes("application/json")) {
            const data = await res.json();
            if (Array.isArray(data)) return data.join("; ");
            if (typeof data === "string") return data;
            if (data?.errors) {
                if (Array.isArray(data.errors)) return data.errors.join("; ");
                if (typeof data.errors === "object") {
                    return Object.values(data.errors).flat().join("; ");
                }
            }
            if (data?.message) return data.message;
            return JSON.stringify(data);
        } else {
            return await res.text();
        }
    } catch {
        return "";
    }
}
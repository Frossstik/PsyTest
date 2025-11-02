import React, { useState } from "react";
import Header from "../components/Header";
import { useAuth } from "../context/AuthContext";

export default function ChangePasswordPage() {
    const { token } = useAuth();

    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState(null); // успех / ошибка

    // URL фронта, который мы передаём в бэк как ConfirmUrl
    // именно туда потом придёт пользователь по ссылке из письма
    const confirmUrl = `${window.location.origin}/confirm-password-change`;

    console.log(confirmUrl);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage(null);

        if (!currentPassword || !newPassword || !confirmPassword) {
            setMessage({ type: "error", text: "Заполните все поля" });
            return;
        }

        if (newPassword !== confirmPassword) {
            setMessage({ type: "error", text: "Пароли не совпадают" });
            return;
        }

        setLoading(true);

        try {
            const res = await fetch(
                `${import.meta.env.VITE_IDENTITY_URL}/change-password`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        Authorization: `Bearer ${token}`,
                    },
                    body: JSON.stringify({
                        currentPassword,
                        newPassword,
                        confirmUrl // бэк вставит email/token/newPassword сам
                    }),
                }
            );

            const text = await res.text();

            if (res.ok) {
                setMessage({
                    type: "success",
                    text: "Письмо для подтверждения отправлено на вашу почту.",
                });
                setCurrentPassword("");
                setNewPassword("");
                setConfirmPassword("");
            } else {
                setMessage({
                    type: "error",
                    text: text || "Не удалось отправить письмо",
                });
            }
        } catch (err) {
            setMessage({
                type: "error",
                text: "Ошибка сети. Попробуйте позже.",
            });
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />

            <main className="max-w-xl mx-auto p-6 mt-8 bg-white shadow rounded-xl">
                <h2 className="text-2xl font-bold mb-4 text-gray-800">
                    Безопасность аккаунта
                </h2>

                <h3 className="text-lg font-semibold mb-2 text-gray-700">
                    Смена пароля
                </h3>
                <p className="text-sm text-gray-600 mb-6">
                    После отправки формы мы вышлем на ваш email письмо
                    с подтверждением смены пароля. Новый пароль начнет
                    действовать только после подтверждения.
                </p>

                {message && (
                    <div
                        className={
                            "mb-4 text-sm px-4 py-2 rounded-lg " +
                            (message.type === "success"
                                ? "bg-green-100 text-green-700 border border-green-300"
                                : "bg-red-100 text-red-700 border border-red-300")
                        }
                    >
                        {message.text}
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block mb-1 font-medium text-gray-800">
                            Текущий пароль
                        </label>
                        <input
                            type="password"
                            value={currentPassword}
                            onChange={(e) => setCurrentPassword(e.target.value)}
                            className="w-full border px-3 py-2 rounded-lg"
                            placeholder="Введите текущий пароль"
                        />
                    </div>

                    <div>
                        <label className="block mb-1 font-medium text-gray-800">
                            Новый пароль
                        </label>
                        <input
                            type="password"
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            className="w-full border px-3 py-2 rounded-lg"
                            placeholder="Введите новый пароль"
                        />
                    </div>

                    <div>
                        <label className="block mb-1 font-medium text-gray-800">
                            Подтверждение нового пароля
                        </label>
                        <input
                            type="password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                            className="w-full border px-3 py-2 rounded-lg"
                            placeholder="Повторите новый пароль"
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className={`w-full text-center px-4 py-2 rounded-lg transition
                            ${loading
                                ? "bg-gray-300 text-gray-600 cursor-not-allowed"
                                : "bg-[color:var(--color-brand)] text-white hover:bg-[color:var(--color-brand-dark)]"
                            }`}
                    >
                        {loading ? "Отправка..." : "Сменить пароль"}
                    </button>
                </form>
            </main>
        </div>
    );
}

import React, { useEffect, useRef, useState } from "react";
import Header from "../components/Header";

export default function ConfirmPasswordChangePage() {
    const [status, setStatus] = useState({
        loading: true,
        ok: null,
        message: "Подтверждаем смену пароля...",
    });

    // флаг "мы уже стреляли запросом"
    const didRunRef = useRef(false);

    useEffect(() => {
        if (didRunRef.current) {
            // уже отправляли запрос → не дублируем
            return;
        }
        didRunRef.current = true;

        // читаем параметры из ссылки письма
        const params = new URLSearchParams(window.location.search);
        const email = params.get("email");
        const token = params.get("token");
        const newPassword = params.get("newPassword");

        if (!email || !token || !newPassword) {
            setStatus({
                loading: false,
                ok: false,
                message: "Некорректная ссылка подтверждения.",
            });
            return;
        }

        fetch(`${import.meta.env.VITE_IDENTITY_URL}/confirm-password-change`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                email,
                token,
                newPassword,
            }),
        })
            .then(async (res) => {
                const text = await res.text();

                if (res.ok) {
                    setStatus({
                        loading: false,
                        ok: true,
                        message:
                            "Пароль успешно изменён. Теперь вы можете войти с новым паролем.",
                    });
                } else {
                    // тут удобно вывести, что сказал бэкенд:
                    setStatus({
                        loading: false,
                        ok: false,
                        message:
                            text ||
                            "Не удалось подтвердить смену пароля. Возможно, ссылка уже использована.",
                    });
                }
            })
            .catch(() => {
                setStatus({
                    loading: false,
                    ok: false,
                    message: "Ошибка сети при подтверждении.",
                });
            });
    }, []);

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />

            <main className="max-w-xl mx-auto p-6 mt-8 bg-white shadow rounded-xl text-center">
                <h2 className="text-2xl font-bold mb-4 text-gray-800">
                    Подтверждение смены пароля
                </h2>

                <div
                    className={
                        "px-4 py-3 rounded-lg border " +
                        (status.loading
                            ? "bg-gray-100 text-gray-700 border-gray-300"
                            : status.ok
                                ? "bg-green-100 text-green-700 border-green-300"
                                : "bg-red-100 text-red-700 border-red-300")
                    }
                >
                    {status.message}
                </div>

                {status.ok && (
                    <div className="mt-6 text-sm text-gray-600">
                        Теперь вы можете перейти на страницу входа
                        и авторизоваться с новым паролем.
                    </div>
                )}
            </main>
        </div>
    );
}

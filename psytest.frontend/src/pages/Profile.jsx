import React, { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import Header from "../components/Header";
import { Link } from "react-router-dom";

export default function Profile() {
    const [profile, setProfile] = useState(null);
    const [history, setHistory] = useState([]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const pageSize = 5;

    const { token, logout } = useAuth();

    useEffect(() => {
        if (token) {
            fetch(`${import.meta.env.VITE_IDENTITY_URL}/profile`, {
                headers: { Authorization: `Bearer ${token}` },
            })
                .then((res) => {
                    if (!res.ok) throw new Error("Unauthorized");
                    return res.json();
                })
                .then((data) => {
                    setProfile(data);
                    loadHistory(data.id, 1);
                })
                .catch(() => logout());
        }
    }, [token, logout]);

    const loadHistory = (userId, currentPage) => {
        fetch(
            `${import.meta.env.VITE_MAIN_URL}/Results/history/${userId}?page=${currentPage}&pageSize=${pageSize}`,
            { headers: { Authorization: `Bearer ${token}` } }
        )
            .then((res) => (res.ok ? res.json() : { items: [], total: 0 }))
            .then((data) => {
                const sorted = data.items
                    ? [...data.items].sort(
                        (a, b) => new Date(b.completedAt) - new Date(a.completedAt)
                    )
                    : [];
                setHistory(sorted);
                setTotalPages(data.totalPages || 1);
                setPage(currentPage);
            })
            .catch((err) => console.error("Ошибка загрузки истории:", err));
    };

    if (!profile) return <div className="p-6">Загрузка...</div>;

    return (
        <div className="min-h-screen bg-gray-50">
            <Header profile={profile} />

            {/* 🔹 Блок профиля */}
            <main className="max-w-xl mx-auto p-6 mt-8 bg-white shadow rounded-xl">
                <h2 className="text-2xl font-bold mb-4 text-gray-800">Профиль</h2>
                <p><b>Email:</b> {profile.email}</p>
                <p><b>Имя:</b> {profile.firstName}</p>
                <p><b>Фамилия:</b> {profile.lastName}</p>
                <p><b>Телефон:</b> {profile.phoneNumber || "не указан"}</p>
                <p><b>Email подтверждён:</b> {profile.emailConfirmed ? "Да" : "Нет"}</p>
                <p><b>Телефон подтверждён:</b> {profile.phoneNumberConfirmed ? "Да" : "Нет"}</p>

                <div className="flex flex-col sm:flex-row gap-3 mt-5">
                    <Link
                        to="/profile/edit"
                        className="flex-1 text-center bg-[color:var(--color-brand)] text-white px-4 py-2 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                    >
                        Изменить информацию
                    </Link>

                    <Link
                        to="/profile/security"
                        className="flex-1 text-center bg-[color:var(--color-brand)] text-white px-4 py-2 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                    >
                        Смена пароля
                    </Link>
                </div>
            </main>

            {/* 🔹 История тестов */}
            <section className="max-w-xl mx-auto p-6 mt-6 bg-white shadow rounded-xl">
                <h2 className="text-xl font-bold mb-4 text-gray-800">История тестов</h2>

                {history.length === 0 ? (
                    <p className="text-gray-600">Тесты ещё не проходились</p>
                ) : (
                    <>
                        <ul className="space-y-4">
                            {history.map((item) => (
                                <li key={item.sessionId} className="border-b pb-3 last:border-none">
                                    <div className="flex items-center justify-between">
                                        <div>
                                            <p className="font-semibold">
                                                {item.testName || `${item.resultText.slice(0, 50)}...`}
                                            </p>
                                            <p className="text-sm text-gray-500">
                                                {item.completedAt
                                                    ? new Date(item.completedAt).toLocaleString()
                                                    : "В процессе"}
                                            </p>
                                        </div>
                                        <Link
                                            to={`/results/${item.sessionId}`}
                                            className="text-[color:var(--color-brand)] hover:underline"
                                        >
                                            Открыть
                                        </Link>
                                    </div>
                                </li>
                            ))}
                        </ul>

                        {/* 🔹 Пагинация */}
                        <div className="flex justify-center items-center gap-4 mt-6">
                            <button
                                onClick={() => loadHistory(profile.id, page - 1)}
                                disabled={page === 1}
                                className={`px-3 py-1 rounded ${page === 1
                                    ? "bg-gray-200 text-gray-500 cursor-not-allowed"
                                    : "bg-[color:var(--color-brand)] text-white hover:bg-[color:var(--color-brand-dark)]"
                                    }`}
                            >
                                ← Назад
                            </button>

                            <span className="text-gray-700">
                                Страница {page} из {totalPages}
                            </span>

                            <button
                                onClick={() => loadHistory(profile.id, page + 1)}
                                disabled={page === totalPages}
                                className={`px-3 py-1 rounded ${page === totalPages
                                    ? "bg-gray-200 text-gray-500 cursor-not-allowed"
                                    : "bg-[color:var(--color-brand)] text-white hover:bg-[color:var(--color-brand-dark)]"
                                    }`}
                            >
                                Вперёд →
                            </button>
                        </div>
                    </>
                )}
            </section>
        </div>
    );
}

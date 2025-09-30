import React, { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";

export default function Home() {
    const [tests, setTests] = useState([]);
    const [search, setSearch] = useState("");
    const { logout, token } = useAuth();

    // Допустим email хранится в токене (или временно в localStorage)
    const userEmail = localStorage.getItem("userEmail") || "user@example.com";

    useEffect(() => {
        fetch(`${import.meta.env.VITE_MAIN_URL}/api/tests`)
            .then((res) => res.json())
            .then((data) => setTests(data));
    }, []);

    const filteredTests = tests.filter((t) =>
        t.name.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Хедер */}
            <header className="flex items-center justify-between bg-white px-6 py-4 shadow">
                <div className="flex items-center space-x-4">
                    <h1 className="text-xl font-bold text-cyan-700">PsyTest</h1>
                    <input
                        type="text"
                        placeholder="Поиск тестов..."
                        className="px-3 py-1 border rounded-lg focus:ring-2 focus:ring-cyan-500"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
                <div className="flex items-center space-x-4">
                    <span className="text-gray-700">{userEmail}</span>
                    <button
                        onClick={logout}
                        className="bg-cyan-600 text-white px-3 py-1 rounded-lg hover:bg-cyan-700"
                    >
                        Выйти
                    </button>
                </div>
            </header>

            {/* Контент */}
            <main className="p-6">
                <h2 className="text-2xl font-bold mb-4">Список тестов</h2>
                <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {filteredTests.map((t) => (
                        <div
                            key={t.id}
                            className="bg-white p-6 rounded-xl shadow hover:shadow-lg transition"
                        >
                            <h3 className="text-lg font-semibold">{t.name}</h3>
                            <p className="text-gray-600">{t.shortDescription}</p>
                            <button className="mt-4 w-full bg-cyan-500 text-white py-2 rounded-lg hover:bg-cyan-600">
                                Пройти
                            </button>
                        </div>
                    ))}
                </div>
            </main>
        </div>
    );
}

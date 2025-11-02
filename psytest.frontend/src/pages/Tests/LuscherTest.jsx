import React, { useEffect, useState } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Header from "../../components/Header";

const COLORS = [
    { id: 1, hex: "#333670" },
    { id: 2, hex: "#1D9F73" },
    { id: 3, hex: "#FB4918" },
    { id: 4, hex: "#FBDC18" },
    { id: 5, hex: "#ED1254" },
    { id: 6, hex: "#B55F21" },
    { id: 7, hex: "#000000" },
    { id: 0, hex: "#D8D8D4" },
];

export default function LuscherTest() {
    const { id } = useParams();
    const [searchParams] = useSearchParams();
    const sessionId = searchParams.get("sessionId");
    const { token } = useAuth();
    const navigate = useNavigate();

    const [step, setStep] = useState("positive");
    const [positive, setPositive] = useState([]);
    const [negative, setNegative] = useState([]);
    const [shuffled, setShuffled] = useState([...COLORS]);

    // восстановление прогресса
    useEffect(() => {
        const saved = JSON.parse(localStorage.getItem(`luscher-${sessionId}`));
        if (saved) {
            const now = Date.now();
            if (now - saved.timestamp < 20 * 60 * 1000) {
                setStep(saved.step);
                setPositive(saved.positive || []);
                setNegative(saved.negative || []);
                setShuffled(saved.shuffled || [...COLORS]);
                return;
            } else {
                localStorage.removeItem(`luscher-${sessionId}`);
            }
        }
        setShuffled([...COLORS].sort(() => Math.random() - 0.5));
    }, [sessionId]);

    // сохраняем прогресс
    const saveProgress = (newState) => {
        const state = {
            step,
            positive,
            negative,
            shuffled,
            timestamp: Date.now(),
            ...newState,
        };
        localStorage.setItem(`luscher-${sessionId}`, JSON.stringify(state));
    };

    const handlePick = (colorId) => {
        if (step === "positive") {
            const updated = [...positive, colorId];
            setPositive(updated);
            saveProgress({ positive: updated });
            if (updated.length === COLORS.length) {
                const newShuffled = [...COLORS].sort(() => Math.random() - 0.5);
                setStep("negative");
                setShuffled(newShuffled);
                saveProgress({ step: "negative", shuffled: newShuffled });
            }
        } else if (step === "negative") {
            const updated = [...negative, colorId];
            setNegative(updated);
            saveProgress({ negative: updated });
            if (updated.length === COLORS.length) {
                setStep("done");
                saveProgress({ step: "done" });
                submitResults(updated);
            }
        }
    };

    const submitResults = async (finalNegative) => {
        const dto = {
            positive,
            negative: (finalNegative || negative).slice().reverse()
        };

        const res = await fetch(
            `${import.meta.env.VITE_MAIN_URL}/Tests/${sessionId}/luscher`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify(dto),
            }
        );

        if (res.ok) {
            const result = await res.json();
            localStorage.removeItem(`luscher-${sessionId}`);
            navigate(`/results/${sessionId}`, { state: result });
        } else {
            const err = await res.text();
            console.error("❌ Ошибка сохранения:", err);
            alert("Ошибка при сохранении результатов");
        }
    };

    const resetTest = () => {
        localStorage.removeItem(`luscher-${sessionId}`);
        setStep("positive");
        setPositive([]);
        setNegative([]);
        setShuffled([...COLORS].sort(() => Math.random() - 0.5));
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />
            <main className="p-6 max-w-6xl mx-auto">
                <h2 className="text-2xl font-bold mb-4">
                    {step === "positive"
                        ? "Выберите цвета по привлекательности"
                        : step === "negative"
                            ? "Выберите цвета по отталкивающему восприятию"
                            : "Завершено"}
                </h2>

                {step !== "done" && (
                    <>
                        <div className="flex flex-wrap gap-0 justify-between">
                            {shuffled.map((c) => (
                                <button
                                    key={c.id}
                                    onClick={() => handlePick(c.id)}
                                    className="w-32 h-52 rounded-xl shadow-xl transition transform hover:scale-105"
                                    style={{
                                        backgroundColor: c.hex,
                                        opacity:
                                            step === "positive"
                                                ? positive.includes(c.id)
                                                    ? 0
                                                    : 1
                                                : negative.includes(c.id)
                                                    ? 0
                                                    : 1,
                                    }}
                                    disabled={
                                        step === "positive"
                                            ? positive.includes(c.id)
                                            : negative.includes(c.id)
                                    }
                                />
                            ))}
                        </div>

                        {/* кнопка сброса */}
                        <div className="mt-6 flex justify-center">
                            <button
                                onClick={resetTest}
                                className="px-4 py-2 bg-[color:var(--color-brand)] text-white rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                            >
                                Сбросить тест
                            </button>
                        </div>
                    </>
                )}
            </main>
        </div>
    );
}

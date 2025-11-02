import React, { useEffect, useState } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Header from "../../components/Header";

export default function BaiTest() {
    const { id } = useParams(); // testId
    const [searchParams] = useSearchParams();
    const sessionId = searchParams.get("sessionId");
    const { token } = useAuth();
    const navigate = useNavigate();

    const QUESTIONS = [
        "Ощущение онемения или покалывания в теле",
        "Ощущение жары",
        "Дрожь в ногах",
        "Неспособность расслабиться",
        "Страх, что произойдет самое плохое",
        "Головокружение или ощущение легкости в голове",
        "Ускоренное сердцебиение",
        "Неустойчивость",
        "Ощущение ужаса",
        "Нервозность",
        "Дрожь в руках",
        "Ощущение удушья",
        "Шаткость походки",
        "Страх утраты контроля",
        "Затрудненность дыхания",
        "Страх смерти",
        "Испуг",
        "Желудочно-кишечные расстройства",
        "Обмороки",
        "Приливы крови к лицу",
        "Потоотделение (не вызванное жарой)",
    ];

    const OPTIONS = [
        "Совсем не беспокоил",
        "Слегка. Не слишком меня беспокоил",
        "Умеренно. Это было неприятно, но я мог это переносить",
        "Очень сильно. Я с трудом мог это выносить"
    ];

    const [answers, setAnswers] = useState(Array(21).fill(null));

    // 🔹 Загружаем из localStorage (если пользователь выходил)
    useEffect(() => {
        const saved = JSON.parse(localStorage.getItem(`bai-${sessionId}`));
        if (saved) setAnswers(saved);
    }, [sessionId]);

    // 🔹 Сохраняем прогресс
    useEffect(() => {
        localStorage.setItem(`bai-${sessionId}`, JSON.stringify(answers));
    }, [answers, sessionId]);

    const handleAnswer = (qIndex, value) => {
        const updated = [...answers];
        updated[qIndex] = value;
        setAnswers(updated);
    };

    const handleSubmit = async () => {
        if (answers.includes(null)) {
            alert("Пожалуйста, ответьте на все 21 вопрос");
            return;
        }

        const dto = { answers };

        const res = await fetch(
            `${import.meta.env.VITE_MAIN_URL}/Tests/${sessionId}/bai`,
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
            localStorage.removeItem(`bai-${sessionId}`);
            navigate(`/results/${sessionId}`, { state: result });
        } else {
            const err = await res.text();
            console.error("❌ Ошибка сохранения:", err);
            alert("Ошибка при сохранении результатов");
        }
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />
            <main className="max-w-5xl mx-auto p-6">
                <h2 className="text-2xl font-bold mb-6">Шкала тревожности Бека (BAI)</h2>
                <p className="text-gray-600 mb-6">
                    Отметьте, насколько сильно вы испытывали каждое из ощущений за последнюю неделю:
                </p>

                <div className="space-y-8">
                    {QUESTIONS.map((q, i) => (
                        <div
                            key={i}
                            className="bg-white shadow p-5 rounded-xl border border-gray-200"
                        >
                            <p className="font-medium mb-4 text-gray-800">
                                {i + 1}. {q}
                            </p>
                            <div className="flex flex-col gap-2">
                                {OPTIONS.map((label, idx) => (
                                    <button
                                        key={idx}
                                        onClick={() => handleAnswer(i, idx)}
                                        className={`text-left px-4 py-3 rounded-lg border transition ${answers[i] === idx
                                                ? "bg-[color:var(--color-brand)] text-white border-[color:var(--color-brand)]"
                                                : "bg-gray-100 hover:bg-gray-200 border-gray-300 text-gray-800"
                                            }`}
                                    >
                                        {label}
                                    </button>
                                ))}
                            </div>
                        </div>
                    ))}
                </div>

                <button
                    onClick={handleSubmit}
                    className="mt-10 px-6 py-3 bg-[color:var(--color-brand)] text-white rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                >
                    Завершить тест
                </button>
            </main>
        </div>
    );
}
